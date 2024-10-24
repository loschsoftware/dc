﻿using Dassie.Aot;
using Dassie.CodeGeneration.Auxiliary;
using Dassie.Configuration;
using Dassie.Configuration.Macros;
using Dassie.Data;
using Dassie.Errors;
using Dassie.Helpers;
using Dassie.Meta;
using Dassie.Unmanaged;
using Dassie.Validation;
using Microsoft.NET.HostModel.AppHost;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Xml.Serialization;

namespace Dassie.Cli.Commands;

// Does not implement ICompilerCommand because it is not actually a proper command (has no name)
internal static class CompileCommand
{
    public static int Compile(string[] args, DassieConfig overrideSettings = null)
    {
        Stopwatch sw = new();
        sw.Start();

        DassieConfig config = null;

        if (File.Exists("dsconfig.xml"))
        {
            XmlSerializer xmls = new(typeof(DassieConfig));
            using StreamReader sr = new("dsconfig.xml");

            try
            {
                config = (DassieConfig)xmls.Deserialize(sr);
            }
            catch
            {
                // If file is invalid, it will get caught in ConfigValidation.Validate
            }

            foreach (ErrorInfo error in ConfigValidation.Validate("dsconfig.xml"))
                EmitGeneric(error);
        }

        string asmName = "";
        if (args.Where(File.Exists).Any())
            asmName = Path.GetFileNameWithoutExtension(args.Where(File.Exists).First());

        config ??= new();
        config.AssemblyName ??= asmName;

        string[] documentArgs = args.Where(a => a.StartsWith("--Document:")).ToArray();
        args = args.Where(a => !documentArgs.Contains(a)).ToArray();

        CommandLineOptionParser.ParseOptions(ref args, config);

        if (overrideSettings != null)
            config = overrideSettings;

        if (!string.IsNullOrEmpty(config.CompilerMessageRedirectionFile))
        {
            StreamWriter messageWriter = new(config.CompilerMessageRedirectionFile);
            InfoOut = new([messageWriter]);
            WarnOut = new([messageWriter]);
            ErrorOut = new([messageWriter]);
        }

        MacroParser parser = new();
        parser.ImportMacros(MacroGenerator.GenerateMacrosForProject(config));
        parser.Normalize(config);

        ProjectFileCompatibilityTool.VerifyFormatVersionCompatibility(config);

        string[] files = args.Where(s => !s.StartsWith("-") && !s.StartsWith("/") && !s.StartsWith("--")).Select(PatternToFileList).SelectMany(f => f).Select(Path.GetFullPath).ToArray();

        if (args.Where(s => (s.StartsWith("-") || s.StartsWith("/") || s.StartsWith("--")) && s.EndsWith("diagnostics")).Any())
            GlobalConfig.AdvancedDiagnostics = true;

        if (!string.IsNullOrEmpty(config.BuildOutputDirectory))
        {
            Directory.CreateDirectory(config.BuildOutputDirectory);
            Directory.SetCurrentDirectory(config.BuildOutputDirectory);
        }

        string assembly = Path.Combine(config.BuildOutputDirectory ?? "", $"{config.AssemblyName}.dll");
        string msgPrefix = MessagePrefix;

        if (config.References != null)
        {
            foreach (PackageReference packRef in config.References.Where(r => r is PackageReference).Cast<PackageReference>())
            {
                ReferenceHandler.HandlePackageReference(packRef, config);
            }

            foreach (ProjectReference projRef in config.References.Where(r => r is ProjectReference).Cast<ProjectReference>())
            {
                MessagePrefix = Path.GetDirectoryName(projRef.ProjectFile).Split('\\').Last();

                if (!ReferenceHandler.HandleProjectReference(projRef, config, Path.GetFullPath(".\\")))
                    return -1;
            }

            MessagePrefix = msgPrefix;
        }

        if (config.CacheSourceFiles)
        {
            if (File.Exists("dsconfig.xml"))
                files = files.Append("dsconfig.xml").ToArray();

            if (Directory.Exists(".cache")
                && Directory.GetFiles(".cache").Select(Path.GetFileName).SequenceEqual(files.Select(Path.GetFileName)))
            {
                byte[] cachedFiles = Directory.GetFiles(".cache").Select(File.ReadAllBytes).SelectMany(f => f).ToArray();
                byte[] currentFiles = files.Select(File.ReadAllBytes).SelectMany(f => f).ToArray();

                if (cachedFiles.SequenceEqual(currentFiles) && File.Exists(assembly))
                {
                    sw.Stop();

                    if (args.Any(a => a == "-elapsed") || config.MeasureElapsedTime)
                        Console.WriteLine($"\r\nElapsed time: {sw.Elapsed.TotalMilliseconds} ms");

                    return 0;
                }
            }

            var di = Directory.CreateDirectory(".cache");
            di.Attributes |= FileAttributes.Hidden;

            foreach (string file in files)
                File.Copy(file, Path.Combine(".cache", Path.GetFileName(file)), true);

            if (File.Exists("dsconfig.xml"))
                File.Copy("dsconfig.xml", Path.Combine(".cache", "dsconfig.xml"), true);
        }

        List<InputDocument> documents = files.Select(f => new InputDocument(File.ReadAllText(f), f)).ToList();
        documents.AddRange(DocumentCommandLineManager.ExtractDocuments(documentArgs));

        // Step 1
        CompileSource(documents, config);
        VisitorStep1 = Context;

        if (config.Verbosity >= 1)
            EmitBuildLogMessage("Performing second pass.");

        // Step 2
        IEnumerable<ErrorInfo[]> errors = CompileSource(documents, config);

        string resFile = "";

        if ((Context.Configuration.Resources ?? []).Any(r => r is UnmanagedResource))
        {
            resFile = ((UnmanagedResource)Context.Configuration.Resources.First(r => r is UnmanagedResource)).Path;

            if ((Context.Configuration.Resources ?? []).Where(r => r is UnmanagedResource).Count() > 1)
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0068_MultipleUnmanagedResources,
                    "An assembly can only contain one unmanaged resource file.",
                    "dsconfig.xml");
            }
        }
        else if (Context.Configuration.VersionInfo != null)
        {
            EmitMessage(
                0, 0, 0,
                DS0070_AvoidVersionInfoTag,
                $"Using the 'VersionInfo' tag in dsconfig.xml worsens compilation performance. Consider precompiling your version info and including it as an unmanaged resource.",
                "dsconfig.xml");

            string rc = WinSdkHelper.GetToolPath("rc.exe");

            if (string.IsNullOrEmpty(rc))
            {
                EmitWarningMessage(
                    0, 0, 0,
                    DS0069_WinSdkToolNotFound,
                    $"The Windows SDK tool 'rc.exe' could not be located. Setting version information failed. Consider precompiling your version info and including it as an unmanaged resource.",
                    "dsconfig.xml");

                return -1;
            }

            Guid guid = Guid.NewGuid();

            string rcPath = Path.ChangeExtension(config.AssemblyName, "rc");
            ResourceScriptWriter rsw = new(rcPath);

            rsw.BeginVersionInfo();
            rsw.AddFileVersion(Context.Configuration.VersionInfo.FileVersion);
            rsw.AddProductVersion(Context.Configuration.VersionInfo.Version);

            rsw.Begin();
            rsw.AddStringFileInfo(
                Context.Configuration.VersionInfo.Company,
                Context.Configuration.VersionInfo.Description,
                Context.Configuration.VersionInfo.FileVersion,
                Context.Configuration.VersionInfo.InternalName,
                Context.Configuration.VersionInfo.Copyright,
                Context.Configuration.VersionInfo.Trademark,
                Context.Configuration.VersionInfo.Product,
                Context.Configuration.VersionInfo.Version
                );

            rsw.End();

            if (!string.IsNullOrEmpty(Context.Configuration.VersionInfo.ApplicationIcon) && !File.Exists(Context.Configuration.VersionInfo.ApplicationIcon))
            {
                EmitErrorMessage(
                   0, 0, 0,
                   DS0069_WinSdkToolNotFound,
                   $"The specified icon file '{Context.Configuration.VersionInfo.ApplicationIcon}' could not be found.",
                   "dsconfig.xml");

                return -1;
            }

            if (File.Exists(Context.Configuration.VersionInfo.ApplicationIcon ?? ""))
                rsw.AddMainIcon(Context.Configuration.VersionInfo.ApplicationIcon);

            rsw.Dispose();

            ProcessStartInfo psi = new()
            {
                FileName = rc,
                Arguments = rcPath,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process.Start(psi).WaitForExit();

            resFile = Path.ChangeExtension(rcPath, ".res");

            if (!args.Where(s => (s.StartsWith("-") || s.StartsWith("/") || s.StartsWith("--")) && s.EndsWith("rc")).Any())
                File.Delete(rcPath);
        }

        if (!string.IsNullOrEmpty(config.AssemblyManifest) && File.Exists(config.AssemblyManifest))
        {
            // TODO: Include .manifest file
        }

        foreach (Resource res in Context.Configuration.Resources ?? Array.Empty<Resource>())
            AddResource(res, Directory.GetCurrentDirectory());

        if (Context.Files.All(f => f.Errors.Count == 0) && VisitorStep1.Files.All(f => f.Errors.Count == 0))
        {
            NativeResource[] resources = null;

            if (!string.IsNullOrEmpty(resFile))
            {
                resources = [new()
                {
                    Data = File.ReadAllBytes(resFile),
                    Kind = ResourceKind.Version
                }];
            }

            ManagedPEBuilder peBuilder = CreatePEBuilder(Context.EntryPoint, resources, assembly, config.Configuration == ApplicationConfiguration.Debug || config.CreatePdb);

            BlobBuilder peBlob = new();
            peBuilder.Serialize(peBlob);

            FileStream fs = new(Path.GetFileName(assembly), FileMode.Create, FileAccess.Write);
            peBlob.WriteContentTo(fs);
            fs.Dispose();

            if (config.ApplicationType != ApplicationType.Library && config.GenerateNativeAppHost)
            {
                string executableExtension = OperatingSystem.IsWindows() ? "exe" : "";
                string frameworkBaseDir = Directory.GetDirectories(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet", "sdk")).Last();
                string asmPath = Path.GetFileName(assembly);

                HostWriter.CreateAppHost(
                    Directory.GetFiles(Path.Combine(frameworkBaseDir, "AppHostTemplate")).First(),
                    Path.ChangeExtension(asmPath, executableExtension),
                    asmPath,
                    config.ApplicationType == ApplicationType.WinExe,
                    asmPath);
            }

            if (Context.Configuration.Runtime == Configuration.Runtime.Aot)
            {
                AotCompiler compiler = new(Context.Configuration, "dsconfig.xml");
                compiler.Compile();
            }
        }

        string coreLib = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dassie.Core.dll");

        if (Path.GetFullPath(Directory.GetCurrentDirectory()) != Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory))
        {
            try
            {
                File.Copy(coreLib, Path.Combine(Directory.GetCurrentDirectory(), "Dassie.Core.dll"), true);
            }
            catch (IOException) { }
        }

        foreach (string dependency in Context.ReferencedAssemblies.Select(a => a.Location))
        {
            if (Path.GetFullPath(Directory.GetCurrentDirectory()) != Path.GetFullPath(Path.GetDirectoryName(dependency)))
            {
                try
                {
                    File.Copy(dependency, Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileName(dependency)), true);
                }
                catch (IOException) { }
            }
        }

        RuntimeConfigWriter.GenerateRuntimeConfigFile(Path.GetFileNameWithoutExtension(assembly) + ".runtimeconfig.json");

        sw.Stop();

        if (File.Exists(resFile) && !Context.Configuration.PersistentResourceFile)
            File.Delete(resFile);

        if (Directory.Exists(".temp") && !Context.Configuration.KeepIntermediateFiles)
            Directory.Delete(".temp", true);

        if (Context.Configuration.GenerateILFiles)
        {
            string ildasm = WinSdkHelper.GetFrameworkToolPath("ildasm.exe", "GenerateILFiles") ?? "";

            if (File.Exists(ildasm))
            {
                DirectoryInfo dir = Directory.CreateDirectory("cil");

                ProcessStartInfo psi = new()
                {
                    FileName = ildasm,
                    Arguments = $"{Path.GetFullPath(Path.GetFileName(assembly))} /out={Path.Combine(dir.FullName, Path.GetFileNameWithoutExtension(assembly) + ".il")}",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                Process.Start(psi);
            }
        }

        if (Context.Configuration.MeasureElapsedTime)
            InfoOut.WriteLine($"\r\nElapsed time: {sw.Elapsed.TotalMilliseconds} ms");

        InfoOut?.Dispose();
        WarnOut?.Dispose();
        ErrorOut?.Dispose();

        return errors.Select(e => e.Length).Sum() == 0 ? 0 : -1;
    }

    public static ManagedPEBuilder CreatePEBuilder(MethodInfo entryPoint, NativeResource[] resources, string asmName, bool makePdb)
    {
        MetadataBuilder mb = Context.Assembly.GenerateMetadata(out BlobBuilder ilStream, out BlobBuilder mappedFieldData, out MetadataBuilder pdbBuilder);
        PEHeaderBuilder headerBuilder = new(
            imageBase: 0x00400000,
            imageCharacteristics: Characteristics.ExecutableImage);

        MethodDefinitionHandle handle = default;

        if (entryPoint != null)
        {
            if (!(entryPoint.DeclaringType as TypeBuilder).IsCreated())
                (entryPoint.DeclaringType as TypeBuilder).CreateType();

            handle = MetadataTokens.MethodDefinitionHandle(entryPoint.MetadataToken);
        }

        ResourceSectionBuilder rsb = null;
        if (resources != null)
            rsb = new ResourceBuilder(resources);

        DebugDirectoryBuilder dbgBuilder = null;

        if (makePdb)
            dbgBuilder = GeneratePdb(pdbBuilder, mb.GetRowCounts(), handle, Path.ChangeExtension(Path.GetFileName(asmName), "pdb"));

        ManagedPEBuilder peBuilder = new(
            header: headerBuilder,
            metadataRootBuilder: new(mb),
            ilStream: ilStream,
            mappedFieldData,
            entryPoint: handle,
            nativeResources: rsb,
            debugDirectoryBuilder: dbgBuilder);

        return peBuilder;
    }

    private static DebugDirectoryBuilder GeneratePdb(MetadataBuilder pdbBuilder, ImmutableArray<int> rowCounts, MethodDefinitionHandle entryPointHandle, string fileName)
    {
        BlobBuilder portablePdbBlob = new();
        PortablePdbBuilder portablePdbBuilder = new(pdbBuilder, rowCounts, entryPointHandle);
        BlobContentId pdbContentId = portablePdbBuilder.Serialize(portablePdbBlob);

        using FileStream fileStream = new(fileName, FileMode.Create, FileAccess.Write);
        portablePdbBlob.WriteContentTo(fileStream);

        DebugDirectoryBuilder debugDirectoryBuilder = new DebugDirectoryBuilder();
        debugDirectoryBuilder.AddCodeViewEntry(fileName, pdbContentId, portablePdbBuilder.FormatVersion);
        return debugDirectoryBuilder;
    }

    public static void AddResource(Resource res, string basePath)
    {
        if (!File.Exists(res.Path))
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0067_ResourceFileNotFound,
                $"The resource file '{res.Path}' could not be located.",
                "dsconfig.xml");
        }

        else if (res is UnmanagedResource)
        {
            try
            {
                // TODO: Implement alternative
                //Context.Assembly.DefineUnmanagedResource(File.ReadAllBytes(res.Path));
            }
            catch (ArgumentException)
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0068_MultipleUnmanagedResources,
                    "An assembly can only contain one unmanaged resource file.",
                    "dsconfig.xml");
            }
        }

        else
        {
            ManagedResource mres = (ManagedResource)res;
            string resFile = Path.Combine(basePath, Path.GetFileName(mres.Path));

            File.Copy(mres.Path, resFile, true);
            // TODO: Implent alternative
            //Context.Assembly.AddResourceFile(mres.Name, resFile);
        }
    }

    private static string[] PatternToFileList(string pattern)
    {
        static bool IsFileMatchingPattern(string filePath, string filePattern)
        {
            string fileName = Path.GetFileName(filePath);
            string[] patternSegments = filePattern.Split(new[] { '*', '?' }, StringSplitOptions.RemoveEmptyEntries);

            int index = 0;
            foreach (string segment in patternSegments)
            {
                index = fileName.IndexOf(segment, index, StringComparison.OrdinalIgnoreCase);

                if (index == -1)
                    return false;

                index += segment.Length;
            }

            return true;
        }

        string directory = Path.GetDirectoryName(pattern);
        if (string.IsNullOrEmpty(directory))
            directory = ".\\";

        string filePattern = Path.GetFileName(pattern);
        string[] matchingFiles = Directory.GetFiles(directory, filePattern);

        if (filePattern.Contains("*") || filePattern.Contains("?"))
        {
            matchingFiles = matchingFiles.Where(file =>
                IsFileMatchingPattern(file, filePattern)).ToArray();
        }

        if (matchingFiles.Length == 0)
        {
            if (pattern.All(c => char.IsLetter(c) || c == '-'))
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0100_InvalidCommand,
                    $"Unrecognized command '{pattern}'. Use 'dc help' for a list of available commands.",
                    "dc");

                Environment.Exit(-1);
            }

            EmitErrorMessage(
                0,
                0,
                0,
                DS0048_SourceFileNotFound,
                $"The source file '{filePattern}' could not be found.",
                Path.GetFileName(filePattern));
        }

        return matchingFiles;
    }
}