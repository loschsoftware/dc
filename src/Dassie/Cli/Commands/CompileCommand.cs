﻿using Dassie.Aot;
using Dassie.CodeGeneration;
using Dassie.CodeGeneration.Auxiliary;
using Dassie.Configuration;
using Dassie.Configuration.Macros;
using Dassie.Data;
using Dassie.Errors;
using Dassie.Errors.Devices;
using Dassie.Extensions;
using Dassie.Helpers;
using Dassie.Meta;
using Dassie.Unmanaged;
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
using System.Text;
using static System.Net.Mime.MediaTypeNames;

#pragma warning disable IDE0079
#pragma warning disable IL3000

namespace Dassie.Cli.Commands;

internal class CompileCommand : ICompilerCommand
{
    private static CompileCommand _instance;
    public static CompileCommand Instance => _instance ??= new();

    // All empty because this command can never be called like a regular command
    public string Command => "";
    public string UsageString => "";
    public string Description => "";
    public bool Hidden() => true;

    public int Invoke(string[] args) => Compile(args);
    public int Invoke(string[] args, DassieConfig overrideSettings) => Compile(args, overrideSettings);

    private static int Compile(string[] args, DassieConfig overrideSettings = null)
    {
        string workingDir = Directory.GetCurrentDirectory();
        long stopwatchTimeStamp = Stopwatch.GetTimestamp();

        DassieConfig config = ProjectFileDeserializer.DassieConfig;

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

        MacroParser parser = new();
        parser.ImportMacros(MacroGenerator.GenerateMacrosForProject(config));
        parser.Normalize(config);

        ProjectFileCompatibilityTool.VerifyFormatVersionCompatibility(config);

        string[] files = args.Where(s => !s.StartsWith("-") && !s.StartsWith("/") && !s.StartsWith("--")).Select(PatternToFileList).SelectMany(f => f).Select(Path.GetFullPath).ToArray();

        if (args.Where(s => (s.StartsWith("-") || s.StartsWith("/") || s.StartsWith("--")) && s.EndsWith("diagnostics")).Any())
            GlobalConfig.AdvancedDiagnostics = true;

        string relativePathResolverBaseDir = Directory.GetCurrentDirectory();
        GlobalConfig.RelativePathResolverDirectory = relativePathResolverBaseDir;

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
                if (!ReferenceHandler.HandleProjectReference(projRef, config, Path.GetFullPath("./"), relativePathResolverBaseDir))
                    return -1;
            }

            MessagePrefix = msgPrefix;
        }

        if (config.CacheSourceFiles)
        {
            if (File.Exists(ProjectConfigurationFileName))
                files = files.Append(ProjectConfigurationFileName).ToArray();

            if (Directory.Exists(".cache")
                && Directory.GetFiles(".cache").Select(Path.GetFileName).SequenceEqual(files.Select(Path.GetFileName)))
            {
                byte[] cachedFiles = Directory.GetFiles(".cache").Select(File.ReadAllBytes).SelectMany(f => f).ToArray();
                byte[] currentFiles = files.Select(File.ReadAllBytes).SelectMany(f => f).ToArray();

                if (cachedFiles.SequenceEqual(currentFiles) && File.Exists(assembly))
                {
                    if (args.Any(a => a == "-elapsed") || config.MeasureElapsedTime)
                        Console.WriteLine($"\r\nElapsed time: {Stopwatch.GetElapsedTime(stopwatchTimeStamp).TotalMilliseconds} ms");

                    return 0;
                }
            }

            var di = Directory.CreateDirectory(".cache");
            di.Attributes |= FileAttributes.Hidden;

            foreach (string file in files)
                File.Copy(file, Path.Combine(".cache", Path.GetFileName(file)), true);

            if (File.Exists(ProjectConfigurationFileName))
                File.Copy(ProjectConfigurationFileName, Path.Combine(".cache", ProjectConfigurationFileName), true);
        }

        List<InputDocument> documents = files.Select(f => new InputDocument(File.ReadAllText(f), f)).ToList();
        documents.AddRange(DocumentCommandLineManager.ExtractDocuments(documentArgs));

        // Run analyzers (if enabled)
        if (config.RunAnalyzers)
        {
            string compileDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(workingDir);
            AnalyzeCommand.Instance.Invoke(args);
            Directory.SetCurrentDirectory(compileDir);
        }

        // Step 1
        CompileSource(documents, config);
        VisitorStep1 = Context;
        LineNumberOffset = 0;
        UnionTypeCodeGeneration._createdUnionTypes.Clear();

        if (config.Verbosity >= 1)
            EmitBuildLogMessage("Performing second pass.");

        // Step 2
        IEnumerable<ErrorInfo[]> errors = CompileSource(documents, config).Select(l => l.ToArray());

        string resFile = "";

        if ((Context.Configuration.Resources ?? []).Any(r => r is UnmanagedResource))
        {
            resFile = ((UnmanagedResource)Context.Configuration.Resources.First(r => r is UnmanagedResource)).Path;
            resFile = Path.GetFullPath(Path.Combine(relativePathResolverBaseDir, resFile));

            if ((Context.Configuration.Resources ?? []).Where(r => r is UnmanagedResource).Count() > 1)
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0068_MultipleUnmanagedResources,
                    "An assembly can only contain one unmanaged resource file.",
                    ProjectConfigurationFileName);
            }
        }
        else if (Context.Configuration.VersionInfo != null)
        {
            EmitMessage(
                0, 0, 0,
                DS0070_AvoidVersionInfoTag,
                $"Using the 'VersionInfo' tag in dsconfig.xml worsens compilation performance. Consider precompiling your version info and including it as an unmanaged resource.",
                ProjectConfigurationFileName);

            string rc = WinSdkHelper.GetToolPath("rc.exe");

            if (string.IsNullOrEmpty(rc))
            {
                EmitWarningMessage(
                    0, 0, 0,
                    DS0069_WinSdkToolNotFound,
                    $"The Windows SDK tool 'rc.exe' could not be located. Setting version information failed. Consider precompiling your version info and including it as an unmanaged resource.",
                    ProjectConfigurationFileName);

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
                   ProjectConfigurationFileName);

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
            
            if (!Context.Configuration.PersistentResourceScript)
                File.Delete(rcPath);
        }

        if (!string.IsNullOrEmpty(config.AssemblyManifest) && File.Exists(config.AssemblyManifest))
        {
            // TODO: Include .manifest file
        }

        foreach (Resource res in Context.Configuration.Resources ?? Array.Empty<Resource>())
            AddResource(res, Directory.GetCurrentDirectory(), relativePathResolverBaseDir);

        if (!messages.Any(m => m.Severity == Severity.Error))
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

            ManagedPEBuilder peBuilder = CreatePEBuilder(
                Context.EntryPoint,
                resources,
                assembly,
                config.Configuration == ApplicationConfiguration.Debug || config.CreatePdb,
                config.Platform == Platform.x86);

            BlobBuilder peBlob = new();
            peBuilder.Serialize(peBlob);

            FileStream fs = null;

            try
            {
                fs = new(Path.GetFileName(assembly), FileMode.Create, FileAccess.Write);
            }
            catch (IOException ex)
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0029_FileAccessDenied,
                    $"Output assembly could not be saved: {ex.Message}",
                    "dc");
            }

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
                AotCompiler compiler = new(Context.Configuration, ProjectConfigurationFileName);
                compiler.Compile();
            }
        }

        if (!config.NoStdLib)
        {
            string coreLib = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dassie.Core.dll");

            if (Path.GetFullPath(Directory.GetCurrentDirectory()) != Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory))
            {
                try
                {
                    File.Copy(coreLib, Path.Combine(Directory.GetCurrentDirectory(), "Dassie.Core.dll"), true);
                }
                catch (IOException) { }
            }
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

        if (File.Exists(resFile) && !Context.Configuration.PersistentResourceFile)
            File.Delete(resFile);

        if (Directory.Exists(TemporaryBuildDirectoryName) && !Context.Configuration.KeepIntermediateFiles)
            Directory.Delete(TemporaryBuildDirectoryName, true);

        if (Context.Configuration.GenerateILFiles)
        {
            string ildasm = WinSdkHelper.GetFrameworkToolPath("ildasm.exe", "GenerateILFiles") ?? "";

            if (File.Exists(ildasm))
            {
                DirectoryInfo dir = Directory.CreateDirectory(ILFilesDirectoryName);

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
            WriteLine($"\r\nElapsed time: {Stopwatch.GetElapsedTime(stopwatchTimeStamp).TotalMilliseconds} ms");

        TextWriterBuildLogDevice.InfoOut?.Dispose();
        TextWriterBuildLogDevice.WarnOut?.Dispose();
        TextWriterBuildLogDevice.ErrorOut?.Dispose();

        return errors.SelectMany(e => e).Count(e => e.Severity == Severity.Error);
    }

    public static ManagedPEBuilder CreatePEBuilder(MethodInfo entryPoint, NativeResource[] resources, string asmName, bool makePdb, bool is32Bit)
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

        CorFlags runtimeFlags = CorFlags.ILOnly;

        if (is32Bit)
            runtimeFlags |= CorFlags.Prefers32Bit;

        ManagedPEBuilder peBuilder = new(
            header: headerBuilder,
            metadataRootBuilder: new(mb),
            ilStream: ilStream,
            mappedFieldData,
            entryPoint: handle,
            nativeResources: rsb,
            debugDirectoryBuilder: dbgBuilder,
            flags: runtimeFlags);

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

    public static void AddResource(Resource res, string basePath, string relativePathResolverBasePath)
    {
        res.Path = Path.GetFullPath(Path.Combine(relativePathResolverBasePath, res.Path));

        if (!File.Exists(res.Path))
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0067_ResourceFileNotFound,
                $"The resource file '{res.Path}' could not be located.",
                ProjectConfigurationFileName);
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
                    ProjectConfigurationFileName);
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

    private static int Distance(string str1, string str2)
    {
        if (string.IsNullOrEmpty(str1)) return str2?.Length ?? 0;
        if (string.IsNullOrEmpty(str2)) return str1?.Length ?? 0;

        int len1 = str1.Length;
        int len2 = str2.Length;

        int[,] dp = new int[len1 + 1, len2 + 1];

        for (int i = 0; i <= len1; i++) dp[i, 0] = i;
        for (int j = 0; j <= len2; j++) dp[0, j] = j;

        for (int i = 1; i <= len1; i++)
        {
            for (int j = 1; j <= len2; j++)
            {
                int cost = (str1[i - 1] == str2[j - 1]) ? 0 : 1;

                dp[i, j] = Math.Min(
                    Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                    dp[i - 1, j - 1] + cost);
            }
        }

        return dp[len1, len2];
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
            directory = "./";

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
                IEnumerable<string> cmds = CommandRegistry.Commands.Select(c => c.Command.ToLowerInvariant()).Where(c => c.Length > 0).OrderBy(c => Distance(pattern.ToLowerInvariant(), c));
                int dist = Distance(pattern.ToLowerInvariant(), cmds.First().ToLowerInvariant());

                StringBuilder errorMsg = new();
                errorMsg.Append($"Unrecognized command '{pattern}'. ");

                if (dist < 2)
                    errorMsg.Append($"Did you mean '{cmds.First()}'?");
                else if (dist < 5)
                    errorMsg.Append($"Closest match is '{cmds.First()}'.");
                else
                    errorMsg.Append("Use 'dc help' for a list of available commands.");

                EmitErrorMessage(
                    0, 0, 0,
                    DS0100_InvalidCommand,
                    errorMsg.ToString(),
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