using Dassie.Aot;
using Dassie.Cli;
using Dassie.CodeGeneration;
using Dassie.CodeGeneration.Auxiliary;
using Dassie.Configuration;
using Dassie.Core.Properties;
using Dassie.Data;
using Dassie.Extensions;
using Dassie.Messages;
using Dassie.Meta;
using Dassie.Scripting;
using Dassie.Unmanaged;
using Microsoft.NET.HostModel.AppHost;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;
using SDProcess = System.Diagnostics.Process;

#pragma warning disable IDE0079
#pragma warning disable IL3000
#pragma warning disable CA1822

namespace Dassie.Core.Commands;

internal class CompileCommand : CompilerCommand
{
    private static CompileCommand _instance;
    public static CompileCommand Instance => _instance ??= new();

    public override string Command => "compile";
    public override string Description => StringHelper.CompileCommand_Description;
    public override CommandRole Role => CommandRole.Default;
    public override CommandOptions Options => CommandOptions.Hidden | CommandOptions.NoDirectInvocation;

    public override CommandHelpDetails HelpDetails => GetHelpDetails();
    private CommandHelpDetails GetHelpDetails()
    {
        StringBuilder sb = new();
        sb.AppendLine(HelpCommand.FormatLines(StringHelper.CompileCommand_AdvancedOptionsLine1, true, 4));

        sb.Append("    --<PropertyName>=<Value>".PadRight(50));
        sb.Append(HelpCommand.FormatLines(StringHelper.CompileCommand_AdvancedOptionsLine2, indentWidth: 50));

        sb.Append("    --<ArrayPropertyName>+<Value>".PadRight(50));
        sb.Append(HelpCommand.FormatLines(StringHelper.CompileCommand_AdvancedOptionsLine3, indentWidth: 50));

        sb.Append("    --<PropertyName>::<ChildProperty>=<Value>".PadRight(50));
        sb.Append(HelpCommand.FormatLines(StringHelper.CompileCommand_AdvancedOptionsLine4, indentWidth: 50));

        return new()
        {
            Description = Description,
            Usage = ["dc <Files> [Options]"],
            Options =
            [
                ("Files", StringHelper.CompileCommand_FilesOption),
                ("Options", StringHelper.CompileCommand_OptionsOption)
            ],
            CustomSections = [(StringHelper.CompileCommand_AdvancedOptions, sb.ToString())],
            Examples =
            [
                ("dc file1.ds file2.ds", StringHelper.CompileCommand_Example1),
                ("dc main.ds -v 2 -l", StringHelper.CompileCommand_Example2)
            ]
        };
    }

    public override int Invoke(string[] args) => Compile(args);
    public int Invoke(string[] args, DassieConfig overrideSettings, string assemblyName = null)
    {
        DassieConfig prevConfig = Context?.Configuration;

        try
        {
            return Compile(args, overrideSettings, assemblyName);
        }
        finally
        {
            Context?.Configuration = prevConfig;
        }
    }

    internal static void Abort()
    {
        if (GlobalConfig.BuildDirectoryCreated)
        {
            Directory.SetCurrentDirectory(GlobalConfig.RelativePathResolverDirectory);
            Directory.Delete(Context.Configuration.BuildDirectory, true);
        }

        int msgCount = EmittedMessages.Count(e => e.Severity == Severity.Error);
        Context.Configuration.MaxErrors = 0;

        EmitMessageFormatted(
            0, 0, 0,
            DS0234_CompilationTerminated,
            msgCount == 1 ? nameof(StringHelper.CompileCommand_CompilationAborted1) : nameof(StringHelper.CompileCommand_CompilationAborted),
            msgCount == 1 ? [] : [msgCount],
            CompilerExecutableName);

        Program.Exit(234);
    }

    private static int Compile(string[] args, DassieConfig overrideSettings = null, string assemblyName = null)
    {
        if (!ExtensionLoader.Commands.Contains(Instance))
            return HelpCommand.Instance.Invoke([]);

        string workingDir = Directory.GetCurrentDirectory();
        long stopwatchTimeStamp = Stopwatch.GetTimestamp();

        string asmName = "";
        if (args.Where(File.Exists).Any())
            asmName = Path.GetFileNameWithoutExtension(args.Where(File.Exists).First());

        if (assemblyName != null)
            asmName = assemblyName;

        DassieConfig config = null;

        if (overrideSettings != null)
            config = overrideSettings;
        else
            config = ProjectFileSerializer.DassieConfig;

        config ??= DassieConfig.Default;
        config.AssemblyFileName ??= asmName;

        string[] documentArgs = args.Where(a => a.StartsWith("--Document:")).ToArray();
        args = args.Where(a => !documentArgs.Contains(a)).ToArray();

        CommandLineOptionParser.ParseOptions(ref args, config);

        Context ??= new();
        Context.Configuration = config;

        EmitBuildLogMessageFormatted(nameof(StringHelper.CompileCommand_CompilationStarted), [Math.Clamp(config.Verbosity, 0, 3)], 2);
        EmitDeferredBuildLogMessages();

        ProjectFileCompatibilityTool.VerifyFormatVersionCompatibility(config);

        string[] files = args.TakeWhile(a => a != "--").Where(s => !s.StartsWith('-') && !s.StartsWith('/') && !s.StartsWith("--")).Select(PatternToFileList).SelectMany(f => f).Select(Path.GetFullPath).ToArray();

        // Execute script file (.dsx)
        if (!config.NoScript && files.Any(f => Path.GetExtension(f) == DassieScriptFileExtension))
        {
            if (files.Any(f => Path.GetExtension(f) != DassieScriptFileExtension))
            {
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0286_CompileCommandMixedScriptSourceFiles,
                    nameof(StringHelper.CompileCommand_ScriptFilesMixedWithNonScriptFiles), [],
                    CompilerExecutableName);

                return -1;
            }

            if (files.Length > 1)
            {
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0287_MultipleScriptFilesSpecified,
                    nameof(StringHelper.CompileCommand_MultipleScriptFilesSpecified), [],
                    CompilerExecutableName);

                return -1;
            }

            string[] scriptArgs = [];
            if (args.Contains("--"))
                scriptArgs = args.SkipWhile(a => a != "--").Skip(1).ToArray();

            string scriptFile = files.Single();
            return ScriptRunner.Execute(File.ReadAllText(scriptFile), scriptFile, scriptArgs);
        }

        if (args.Where(s => (s.StartsWith('-') || s.StartsWith('/') || s.StartsWith("--")) && s.EndsWith("diagnostics")).Any())
            GlobalConfig.AdvancedDiagnostics = true;

        string relativePathResolverBaseDir = Directory.GetCurrentDirectory();
        GlobalConfig.RelativePathResolverDirectory = relativePathResolverBaseDir;

        GlobalConfig.BuildDirectoryCreated = !Directory.Exists(config.BuildDirectory);
        config.BuildDirectory ??= "";

        if (!string.IsNullOrEmpty(config.BuildDirectory))
        {
            try
            {
                Directory.CreateDirectory(config.BuildDirectory);
            }
            catch (Exception ex)
            {
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0030_FileAccessDenied,
                    nameof(StringHelper.CompileCommand_CouldNotCreateOutputDirectory), [ex.Message],
                    CompilerExecutableName);
            }
        }

        if (Directory.Exists(config.BuildDirectory))
            Directory.SetCurrentDirectory(config.BuildDirectory);

        string assembly = Path.Combine(config.BuildDirectory, $"{config.AssemblyFileName}.dll");
        string msgPrefix = MessagePrefix;

        ISubsystem subsystem = ExtensionLoader.GetSubsystem(Context.Configuration.ApplicationType);

        if (subsystem.References != null && subsystem.References.Length > 0)
            config.References = [.. config.References ?? [], .. subsystem.References];

        if (config.References != null)
        {
            if (config.References.Length > ushort.MaxValue + 1)
            {
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0075_MetadataLimitExceeded,
                    nameof(StringHelper.CompileCommand_AssemblyTooManyReferences), [config.References.Length, ushort.MaxValue + 1],
                    ProjectConfigurationFileName);
            }

            foreach (PackageReference packRef in config.References.OfType<PackageReference>())
                ReferenceHandler.HandlePackageReference(packRef, config);

            foreach (ProjectReference projRef in config.References.OfType<ProjectReference>())
            {
                if (!ReferenceHandler.HandleProjectReference(projRef, config, Path.GetFullPath("./"), relativePathResolverBaseDir))
                    return -1;
            }

            MessagePrefix = msgPrefix;

            IEnumerable<Configuration.AssemblyReference> asmRefs = config.References.OfType<Configuration.AssemblyReference>();
            if (asmRefs.Count() != asmRefs.DistinctBy(r => Path.GetFullPath(r.AssemblyPath)).Count())
            {
                var duplicates = asmRefs.GroupBy(r => Path.GetFullPath(r.AssemblyPath))
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key);

                foreach (string duplicate in duplicates)
                {
                    EmitErrorMessageFormatted(
                        0, 0, 0,
                        DS0241_DuplicateReference,
                        nameof(StringHelper.CompileCommand_DuplicateReference), [duplicate],
                        ProjectConfigurationFileName);
                }

                config.References = config.References.Where(r => r is not Configuration.AssemblyReference ar || !duplicates.Contains(ar.AssemblyPath)).ToArray();
            }
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
                    if (Context.Configuration.MeasureElapsedTime)
                    {
                        EmitMessageFormatted(
                            0, 0, 0,
                            DS0235_ElapsedTime,
                            nameof(StringHelper.CompileCommand_CompilationFinished), [Stopwatch.GetElapsedTime(stopwatchTimeStamp).TotalMilliseconds],
                            CompilerExecutableName);
                    }

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

        List<Data.Document> documents = files.Select(DocumentHelpers.FromFile).ToList();
        documents.AddRange(DocumentCommandLineManager.ExtractDocuments(documentArgs));
        documents.AddRange(DocumentSourceManager.GetDocuments(Context.Configuration));

        if (Context.Configuration.Verbosity >= 2)
        {
            EmitBuildLogMessageFormatted(nameof(StringHelper.CompileCommand_DeterminedDocumentsToCompile), [], 2);

            if (documents.Count == 0)
                EmitBuildLogMessageFormatted(nameof(StringHelper.CompileCommand_None), [], 2);
            else
            {
                foreach (Data.Document doc in documents)
                    EmitBuildLogMessage($"    - {doc.Name}", 2);
            }
        }

        if (config.DocumentTransformers is DocumentTransformerList dtl && dtl.Transformers?.Count > 0)
            documents = DocumentTransformHandler.Transform(documents, dtl);

        // Run analyzers (if enabled)
        if (config.RunAnalyzers)
        {
            string compileDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(workingDir);
            AnalyzeCommand.Instance.Invoke(args);
            Directory.SetCurrentDirectory(compileDir);
        }

        // Step 1
        CompileSource(documents, config, imports: subsystem.Imports);
        VisitorStep1 = Context;
        LineNumberOffset = 0;
        UnionTypeCodeGeneration._createdUnionTypes.Clear();

        if (EmittedMessages.Any(m => m.Code == DS0107_NoInputFiles))
            return -1;

        EmitBuildLogMessageFormatted(nameof(StringHelper.CompileCommand_StartingSecondPass), [], 2);

        // Step 2
        IEnumerable<MessageInfo[]> errors = CompileSource(documents, config, imports: subsystem.Imports).Select(l => l.ToArray());

        string resFile = "";

        if ((Context.Configuration.Resources ?? []).Any(r => r is UnmanagedResource))
        {
            resFile = ((UnmanagedResource)Context.Configuration.Resources.First(r => r is UnmanagedResource)).Path;
            resFile = Path.GetFullPath(Path.Combine(relativePathResolverBaseDir, resFile));

            if ((Context.Configuration.Resources ?? []).Where(r => r is UnmanagedResource).Count() > 1)
            {
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0069_MultipleUnmanagedResources,
                    nameof(StringHelper.CompileCommand_OnlyOneUnmanagedResourceFile), [],
                    ProjectConfigurationFileName);
            }
        }

        if (Context.Configuration.VersionInfo != null && Context.Configuration.VersionInfo.Count > 0 || !string.IsNullOrEmpty(Context.Configuration.IconFile) || !string.IsNullOrEmpty(Context.Configuration.AssemblyManifest))
        {
            if (!string.IsNullOrEmpty(resFile) && Context.Configuration.VersionInfo != null && Context.Configuration.VersionInfo.Count > 0)
            {
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0091_MalformedConfigurationFile,
                    nameof(StringHelper.CompileCommand_VersionInfoInvalid), [],
                    ProjectConfigurationFileName);
            }
            else
            {
                string rc = WinSdkHelper.GetToolPath("rc.exe");

                if (string.IsNullOrEmpty(rc))
                {
                    EmitWarningMessageFormatted(
                        0, 0, 0,
                        DS0070_WinSdkToolNotFound,
                        nameof(StringHelper.CompileCommand_RCNotFound), [],
                        ProjectConfigurationFileName);

                    return -1;
                }

                int[] lcids = [0];

                if (Context.Configuration.VersionInfo != null && Context.Configuration.VersionInfo.Count > 0)
                {
                    lcids = Context.Configuration.VersionInfo.Select(v =>
                    {
                        if (!string.IsNullOrEmpty(v.Language))
                        {
                            try
                            {
                                return new CultureInfo(v.Language).LCID;
                            }
                            catch (CultureNotFoundException)
                            {
                                if (v.Language.Equals("default", StringComparison.InvariantCultureIgnoreCase))
                                    return 0;

                                EmitErrorMessageFormatted(
                                    0, 0, 0,
                                    DS0090_InvalidDSConfigProperty,
                                    nameof(StringHelper.CompileCommand_InvalidLanguageCode), [v.Language],
                                    ProjectConfigurationFileName);
                            }

                            return 1033;
                        }

                        return v.Lcid;

                    }).ToArray();
                }

                string rcPath = Path.ChangeExtension(config.AssemblyFileName, "rc");
                ResourceScriptWriter rsw = new(rcPath, lcids);

                if (lcids.Distinct().Count() != lcids.Length)
                {
                    List<int> seen = [];
                    foreach (int lcid in lcids)
                    {
                        if (seen.Contains(lcid))
                        {
                            EmitErrorMessageFormatted(
                                0, 0, 0,
                                DS0090_InvalidDSConfigProperty,
                                nameof(StringHelper.CompileCommand_DuplicateVersionInfo), [new CultureInfo(lcid).Name],
                                ProjectConfigurationFileName);
                        }

                        seen.Add(lcid);
                    }
                }

                if (Context.Configuration.VersionInfo != null && Context.Configuration.VersionInfo.Count > 0)
                {
                    rsw.SetLanguage(0);
                    rsw.BeginVersionInfo(subsystem.IsExecutable ? 1 : 2);
                    rsw.AddFileVersion(Context.Configuration.VersionInfo[0].FileVersion);
                    rsw.AddProductVersion(Context.Configuration.VersionInfo[0].Version);

                    rsw.Begin();

                    foreach ((int i, VersionInfo lang) in Context.Configuration.VersionInfo.Index())
                    {
                        rsw.AddStringFileInfo(
                            lcids[i],
                            lang.Company,
                            lang.Description,
                            lang.FileVersion,
                            lang.InternalName,
                            lang.Copyright,
                            lang.Trademark,
                            lang.Product,
                            lang.Version
                            );
                    }

                    rsw.AddVarFileInfo();
                    rsw.End();
                }

                string icoFile = Path.GetFullPath(Path.Combine(relativePathResolverBaseDir, Context.Configuration.IconFile ?? ""));
                string manifest = Path.GetFullPath(Path.Combine(relativePathResolverBaseDir, Context.Configuration.AssemblyManifest ?? ""));

                if (!string.IsNullOrEmpty(Context.Configuration.IconFile) && !File.Exists(icoFile))
                {
                    EmitErrorMessageFormatted(
                       0, 0, 0,
                       DS0068_ResourceFileNotFound,
                       nameof(StringHelper.CompileCommand_IconFileNotFound), [Context.Configuration.IconFile],
                       ProjectConfigurationFileName);

                    return -1;
                }

                if (!string.IsNullOrEmpty(Context.Configuration.AssemblyManifest) && !File.Exists(manifest))
                {
                    EmitErrorMessageFormatted(
                        0, 0, 0,
                        DS0068_ResourceFileNotFound,
                        nameof(StringHelper.CompileCommand_ManifestFileNotFound), [Context.Configuration.AssemblyManifest],
                        ProjectConfigurationFileName);

                    return -1;
                }

                if (File.Exists(icoFile))
                    rsw.AddMainIcon(icoFile);

                if (File.Exists(manifest))
                    rsw.AddManifest(manifest);

                rsw.Dispose();

                ProcessStartInfo psi = new()
                {
                    FileName = rc,
                    Arguments = rcPath,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                SDProcess.Start(psi).WaitForExit();

                resFile = Path.ChangeExtension(rcPath, ".res");

                if (!Context.Configuration.PersistentResourceScript)
                    File.Delete(rcPath);
            }
        }

        foreach (Resource res in Context.Configuration.Resources ?? [])
            AddResource(res, Directory.GetCurrentDirectory(), relativePathResolverBaseDir);

        if (!BuildFailed)
        {
            ResourceExtractor.Resource[] resources = [];

            if (File.Exists(resFile))
            {
                byte[] resourceBytes = File.ReadAllBytes(resFile);
                resources = ResourceExtractor.GetResources(resourceBytes, Path.GetFileName(resFile));
            }

            ManagedPEBuilder peBuilder = CreatePEBuilder(
                Context.EntryPoint,
                resources,
                assembly,
                !config.MinimalOutput && (config.Configuration == ApplicationConfiguration.Debug || config.EmitPdb),
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
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0030_FileAccessDenied,
                    nameof(StringHelper.CompileCommand_AssemblySaveError), [ex.Message],
                    CompilerExecutableName);
            }

            peBlob.WriteContentTo(fs);
            fs.Dispose();

            if (subsystem.IsExecutable && config.GenerateNativeAppHost && !config.MinimalOutput)
            {
                string executableExtension = OperatingSystem.IsWindows() ? "exe" : "";
                string frameworkBaseDir = Directory.GetDirectories(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet", "sdk")).Last();
                string asmPath = Path.GetFileName(assembly);

                HostWriter.CreateAppHost(
                    Directory.GetFiles(Path.Combine(frameworkBaseDir, "AppHostTemplate")).First(),
                    Path.ChangeExtension(asmPath, executableExtension),
                    asmPath,
                    subsystem.WindowsGui,
                    asmPath);
            }

            if (Context.Configuration.Runtime == Configuration.Runtime.Aot)
            {
                AotCompiler compiler = new(Context.Configuration, ProjectConfigurationFileName);
                compiler.Compile();
            }

            if (!config.NoStdLib && !config.MinimalOutput)
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

            if (!config.MinimalOutput)
            {
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
            }

            if (File.Exists(resFile) && !Context.Configuration.PersistentResourceFile)
                File.Delete(resFile);

            if (Directory.Exists(TemporaryBuildDirectoryName) && !Context.Configuration.KeepIntermediateFiles)
                Directory.Delete(TemporaryBuildDirectoryName, true);

            if (Context.Configuration.GenerateILFiles)
            {
                string ildasm = (string)ILDasmPathProperty.Instance.GetValue() ?? "";

                if (!File.Exists(ildasm))
                {
                    EmitErrorMessageFormatted(
                        0, 0, 0,
                        DS0261_ExtensionsLocationPropertyInvalidPath,
                        nameof(StringHelper.CompileCommand_ILDasmPathPrompt), []);
                }
                else
                {
                    DirectoryInfo dir = Directory.CreateDirectory(ILFilesDirectoryName);

                    ProcessStartInfo psi = new()
                    {
                        FileName = ildasm,
                        Arguments = $"{Path.GetFullPath(Path.GetFileName(assembly))} /out={Path.Combine(dir.FullName, Path.GetFileNameWithoutExtension(assembly) + ".il")}",
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };

                    SDProcess.Start(psi);
                }
            }

            if (config.MeasureElapsedTime)
            {
                EmitMessageFormatted(
                    0, 0, 0,
                    DS0235_ElapsedTime,
                    nameof(StringHelper.CompileCommand_CompilationFinished), [Stopwatch.GetElapsedTime(stopwatchTimeStamp).TotalMilliseconds],
                    CompilerExecutableName);
            }
        }
        else if (GlobalConfig.BuildDirectoryCreated)
        {
            Directory.SetCurrentDirectory(workingDir);
            Directory.Delete(config.BuildDirectory);
        }

        return errors.SelectMany(e => e).Count(e => e.Severity == Severity.Error);
    }

    public static ManagedPEBuilder CreatePEBuilder(MethodInfo entryPoint, ResourceExtractor.Resource[] resources, string asmName, bool makePdb, bool is32Bit)
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
            rsb = new ResourceSerializer(resources);

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

        DebugDirectoryBuilder debugDirectoryBuilder = new();
        debugDirectoryBuilder.AddCodeViewEntry(fileName, pdbContentId, portablePdbBuilder.FormatVersion);
        return debugDirectoryBuilder;
    }

    public static void AddResource(Resource res, string basePath, string relativePathResolverBasePath)
    {
        res.Path = Path.GetFullPath(Path.Combine(relativePathResolverBasePath, res.Path));

        if (!File.Exists(res.Path))
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0068_ResourceFileNotFound,
                nameof(StringHelper.CompileCommand_ResourceFileNotFound), [res.Path],
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
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0069_MultipleUnmanagedResources,
                    nameof(StringHelper.CompileCommand_OnlyOneUnmanagedResourceFile), [],
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
                int cost = str1[i - 1] == str2[j - 1] ? 0 : 1;

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
            string[] patternSegments = filePattern.Split(['*', '?'], StringSplitOptions.RemoveEmptyEntries);

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
        string[] matchingFiles = [];

        if (string.IsNullOrEmpty(filePattern))
            filePattern = pattern;

        try
        {
            matchingFiles = Directory.GetFiles(directory, filePattern);
        }
        catch { }

        if (filePattern.Contains('*') || filePattern.Contains('?'))
        {
            matchingFiles = matchingFiles.Where(file =>
                IsFileMatchingPattern(file, filePattern)).ToArray();
        }

        if (matchingFiles.Length == 0)
        {
            if (pattern.All(c => char.IsLetter(c) || c == '-'))
            {
                IEnumerable<string> cmds = ExtensionLoader.Commands.Where(c => !c.Options.HasFlag(CommandOptions.Hidden)).Select(c => c.Command.ToLowerInvariant()).Where(c => c.Length > 0).OrderBy(c => Distance(pattern.ToLowerInvariant(), c));
                int dist = int.MaxValue;

                if (cmds.Any())
                    dist = Distance(pattern.ToLowerInvariant(), cmds.First().ToLowerInvariant());

                StringBuilder errorMsg = new();
                errorMsg.Append(StringHelper.Format(nameof(StringHelper.CompileCommand_UnrecognizedCommand), pattern));

                if (dist < 2)
                    errorMsg.Append(StringHelper.Format(nameof(StringHelper.CompileCommand_DidYouMean), cmds.First()));
                else if (dist < 5)
                    errorMsg.Append(StringHelper.Format(nameof(StringHelper.CompileCommand_ClosestMatch), cmds.First()));
                else if (ExtensionLoader.Commands.Contains(HelpCommand.Instance))
                    errorMsg.Append(StringHelper.CompileCommand_DCHelp);

                EmitErrorMessage(
                    0, 0, 0,
                    DS0101_InvalidCommand,
                    errorMsg.ToString(),
                    CompilerExecutableName);

                Environment.Exit(-1);
            }

            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0049_SourceFileNotFound,
                nameof(StringHelper.CompileCommand_SourceFileNotFound), [filePattern],
                filePattern);
        }

        return matchingFiles;
    }
}