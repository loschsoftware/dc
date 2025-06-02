using Dassie.Configuration;
using Dassie.Meta;
using Dassie.Unmanaged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Dassie.Aot;

/// <summary>
/// Builds the argument list passed to ilc.exe and link.exe to allow ahead-of-time compilation.
/// </summary>
internal class AotCommandLineBuilder
{
    private readonly AotConfig _config;
    private readonly StringBuilder _arglistBuilder = new();
    private readonly StringBuilder _linkerArglistBuilder = new();

    private readonly string _outputDirectory;
    private readonly string _tempOutputDirectory;

    private readonly string _objectFile;
    private readonly string _exportsFile;

    private string _os;
    private string _architecture;

    /// <summary>
    /// Creates a new instance of <see cref="AotCommandLineBuilder"/> based on the specified AOT configuration.
    /// </summary>
    /// <param name="config">The configuration for the current AOT compilation.</param>
    public AotCommandLineBuilder(AotConfig config)
    {
        _config = config;

        //_tempOutputDirectory = Path.Combine(Path.GetFullPath(_config.Config.BuildOutputDirectory), TemporaryBuildDirectoryName, AotBuildDirectoryName);
        //_outputDirectory = Path.Combine(Path.GetFullPath(_config.Config.BuildOutputDirectory), AotBuildDirectoryName);

        _tempOutputDirectory = Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), TemporaryBuildDirectoryName, AotBuildDirectoryName)).FullName;
        _outputDirectory = Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), AotBuildDirectoryName)).FullName;

        _objectFile = Path.Combine(_tempOutputDirectory, $"{_config.Config.AssemblyName}.obj");
        _exportsFile = Path.Combine(_tempOutputDirectory, $"{_config.Config.AssemblyName}.def");

        _os = _config.Config.RuntimeIdentifier.Split('-')[0];
        _architecture = _config.Config.RuntimeIdentifier.Split('-')[1];
    }

    /// <summary>
    /// Generates the arguments passed to ilc.exe.
    /// </summary>
    /// <returns>The argument list ilc.exe is invoked with.</returns>
    public string GenerateIlcArgumentList()
    {
        AddFileSpecificArguments();
        AddCustomReferences();
        AddDefaultReferences();
        AddAdditionalConfig();
        AddFeatureFlags();
        SetRuntimeIdentifier();

        return _arglistBuilder.ToString().Replace(Environment.NewLine, " ");
    }

    /// <summary>
    /// Generates the arguments passed to link.exe
    /// </summary>
    /// <returns>The argument list link.exe is invoked with.</returns>
    public string GenerateLinkerArgumentList(out string linkerPath)
    {
        AddLinkerFileSpecificArguments();
        AddLinkerDefaultArguments(out linkerPath);
        return _linkerArglistBuilder.ToString().Replace(Environment.NewLine, " ");
    }

    private void AddFileSpecificArguments()
    {
        // The input .NET executable to compile
        //string inputFile = Path.Combine(Path.GetFullPath(_config.Config.BuildOutputDirectory), $"{_config.Config.AssemblyName}.dll");
        string inputFile = Path.Combine(Directory.GetCurrentDirectory(), $"{_config.Config.AssemblyName}.dll");

        _arglistBuilder.AppendLine($"\"{inputFile}\"");
        _arglistBuilder.AppendLine($"-o:\"{_objectFile}\"");
        _arglistBuilder.AppendLine($"--exportsfile:\"{_exportsFile}\"");
        _arglistBuilder.AppendLine($"--win32resourcemodule:\"{Path.GetFileNameWithoutExtension(inputFile)}\"");
        _arglistBuilder.AppendLine($"--nosinglewarnassembly:\"{Path.GetFileNameWithoutExtension(inputFile)}\"");
    }

    /// <summary>
    /// Adds references to core framework libraries to the argument list.
    /// </summary>
    private void AddDefaultReferences()
    {
        string sdkDir = Path.Combine(_config.ILCompilerPackageRootDirectory, "sdk");
        IEnumerable<string> sdkFiles = ["System.Private.CoreLib.dll", "System.Private.DisabledReflection.dll", "System.Private.Reflection.Execution.dll", "System.Private.StackTraceMetadata.dll", "System.Private.TypeLoader.dll"];
        sdkFiles = sdkFiles.Select(f => $"-r:\"{Path.Combine(sdkDir, f)}\"");

        string frameworkDir = Path.Combine(_config.ILCompilerPackageRootDirectory, "framework");
        IEnumerable<string> frameworkFiles = ["Microsoft.CSharp.dll", "Microsoft.VisualBasic.Core.dll", "Microsoft.VisualBasic.dll", "Microsoft.Win32.Primitives.dll", "Microsoft.Win32.Registry.dll", "mscorlib.dll", "netstandard.dll", "System.AppContext.dll", "System.Buffers.dll", "System.Collections.Concurrent.dll", "System.Collections.dll", "System.Collections.Immutable.dll", "System.Collections.NonGeneric.dll", "System.Collections.Specialized.dll", "System.ComponentModel.Annotations.dll", "System.ComponentModel.DataAnnotations.dll", "System.ComponentModel.dll", "System.ComponentModel.EventBasedAsync.dll", "System.ComponentModel.Primitives.dll", "System.ComponentModel.TypeConverter.dll", "System.Configuration.dll", "System.Console.dll", "System.Core.dll", "System.Data.Common.dll", "System.Data.DataSetExtensions.dll", "System.Data.dll", "System.Diagnostics.Contracts.dll", "System.Diagnostics.Debug.dll", "System.Diagnostics.DiagnosticSource.dll", "System.Diagnostics.FileVersionInfo.dll", "System.Diagnostics.Process.dll", "System.Diagnostics.StackTrace.dll", "System.Diagnostics.TextWriterTraceListener.dll", "System.Diagnostics.Tools.dll", "System.Diagnostics.TraceSource.dll", "System.Diagnostics.Tracing.dll", "System.dll", "System.Drawing.dll", "System.Drawing.Primitives.dll", "System.Dynamic.Runtime.dll", "System.Formats.Asn1.dll", "System.Formats.Tar.dll", "System.Globalization.Calendars.dll", "System.Globalization.dll", "System.Globalization.Extensions.dll", "System.IO.Compression.Brotli.dll", "System.IO.Compression.dll", "System.IO.Compression.FileSystem.dll", "System.IO.Compression.ZipFile.dll", "System.IO.dll", "System.IO.FileSystem.AccessControl.dll", "System.IO.FileSystem.dll", "System.IO.FileSystem.DriveInfo.dll", "System.IO.FileSystem.Primitives.dll", "System.IO.FileSystem.Watcher.dll", "System.IO.IsolatedStorage.dll", "System.IO.MemoryMappedFiles.dll", "System.IO.Pipelines.dll", "System.IO.Pipes.AccessControl.dll", "System.IO.Pipes.dll", "System.IO.UnmanagedMemoryStream.dll", "System.Linq.dll", "System.Linq.Expressions.dll", "System.Linq.Parallel.dll", "System.Linq.Queryable.dll", "System.Memory.dll", "System.Net.dll", "System.Net.Http.dll", "System.Net.Http.Json.dll", "System.Net.HttpListener.dll", "System.Net.Mail.dll", "System.Net.NameResolution.dll", "System.Net.NetworkInformation.dll", "System.Net.Ping.dll", "System.Net.Primitives.dll", "System.Net.Quic.dll", "System.Net.Requests.dll", "System.Net.Security.dll", "System.Net.ServicePoint.dll", "System.Net.Sockets.dll", "System.Net.WebClient.dll", "System.Net.WebHeaderCollection.dll", "System.Net.WebProxy.dll", "System.Net.WebSockets.Client.dll", "System.Net.WebSockets.dll", "System.Numerics.dll", "System.Numerics.Vectors.dll", "System.ObjectModel.dll", "System.Private.DataContractSerialization.dll", "System.Private.Uri.dll", "System.Private.Xml.dll", "System.Private.Xml.Linq.dll", "System.Reflection.DispatchProxy.dll", "System.Reflection.dll", "System.Reflection.Emit.dll", "System.Reflection.Emit.ILGeneration.dll", "System.Reflection.Emit.Lightweight.dll", "System.Reflection.Extensions.dll", "System.Reflection.Metadata.dll", "System.Reflection.Primitives.dll", "System.Reflection.TypeExtensions.dll", "System.Resources.Reader.dll", "System.Resources.ResourceManager.dll", "System.Resources.Writer.dll", "System.Runtime.CompilerServices.Unsafe.dll", "System.Runtime.CompilerServices.VisualC.dll", "System.Runtime.dll", "System.Runtime.Extensions.dll", "System.Runtime.Handles.dll", "System.Runtime.InteropServices.dll", "System.Runtime.InteropServices.JavaScript.dll", "System.Runtime.InteropServices.RuntimeInformation.dll", "System.Runtime.Intrinsics.dll", "System.Runtime.Loader.dll", "System.Runtime.Numerics.dll", "System.Runtime.Serialization.dll", "System.Runtime.Serialization.Formatters.dll", "System.Runtime.Serialization.Json.dll", "System.Runtime.Serialization.Primitives.dll", "System.Runtime.Serialization.Xml.dll", "System.Security.AccessControl.dll", "System.Security.Claims.dll", "System.Security.Cryptography.Algorithms.dll", "System.Security.Cryptography.Cng.dll", "System.Security.Cryptography.Csp.dll", "System.Security.Cryptography.dll", "System.Security.Cryptography.Encoding.dll", "System.Security.Cryptography.OpenSsl.dll", "System.Security.Cryptography.Primitives.dll", "System.Security.Cryptography.X509Certificates.dll", "System.Security.dll", "System.Security.Principal.dll", "System.Security.Principal.Windows.dll", "System.Security.SecureString.dll", "System.ServiceModel.Web.dll", "System.ServiceProcess.dll", "System.Text.Encoding.CodePages.dll", "System.Text.Encoding.dll", "System.Text.Encoding.Extensions.dll", "System.Text.Encodings.Web.dll", "System.Text.Json.dll", "System.Text.RegularExpressions.dll", "System.Threading.Channels.dll", "System.Threading.dll", "System.Threading.Overlapped.dll", "System.Threading.Tasks.Dataflow.dll", "System.Threading.Tasks.dll", "System.Threading.Tasks.Extensions.dll", "System.Threading.Tasks.Parallel.dll", "System.Threading.Thread.dll", "System.Threading.ThreadPool.dll", "System.Threading.Timer.dll", "System.Transactions.dll", "System.Transactions.Local.dll", "System.ValueTuple.dll", "System.Web.dll", "System.Web.HttpUtility.dll", "System.Windows.dll", "System.Xml.dll", "System.Xml.Linq.dll", "System.Xml.ReaderWriter.dll", "System.Xml.Serialization.dll", "System.Xml.XDocument.dll", "System.Xml.XmlDocument.dll", "System.Xml.XmlSerializer.dll", "System.Xml.XPath.dll", "System.Xml.XPath.XDocument.dll", "WindowsBase.dll"];
        frameworkFiles = frameworkFiles.Select(f => $"-r:\"{Path.Combine(frameworkDir, f)}\"");

        string referencesString = string.Join(Environment.NewLine, sdkFiles.Concat(frameworkFiles));
        _arglistBuilder.AppendLine(referencesString);

        // Dassie Core library
        if (!_config.Config.NoStdLib)
            _arglistBuilder.AppendLine($"-r:\"{Path.GetFullPath("Dassie.Core.dll")}\"");

        Version version = typeof(object).Assembly.GetName().Version;
        string target = version.ToString(2);

        _arglistBuilder.AppendLine($"-r:\"{Path.Combine(_config.RuntimePackageRootDirectory, "runtimes", _config.Config.RuntimeIdentifier, "lib", $"net{target}", "WindowsBase.dll")}\"");
    }

    private void AddCustomReferences()
    {
        foreach (Reference reference in _config.Config.References ?? [])
        {
            if (reference is AssemblyReference a)
            {
                _arglistBuilder.AppendLine($"-r:\"{Path.GetFullPath(Path.Combine(GlobalConfig.RelativePathResolverDirectory, a.AssemblyPath))}\"");
                continue;
            }

            if (reference is ProjectReference p)
            {
                // TODO: Support project references for AOT
                continue;
            }
        }
    }

    private void AddFeatureFlags()
    {
        // TODO: Allow modifying some of these?
        _arglistBuilder.AppendLine(@"--feature:Microsoft.Extensions.DependencyInjection.VerifyOpenGenericServiceTrimmability=true
--feature:System.ComponentModel.TypeConverter.EnableUnsafeBinaryFormatterInDesigntimeLicenseContextSerialization=false
--feature:System.ComponentModel.TypeDescriptor.IsComObjectDescriptorSupported=false
--feature:System.Diagnostics.Tracing.EventSource.IsSupported=false
--feature:System.Globalization.Invariant=true
--feature:System.Globalization.PredefinedCulturesOnly=true
--feature:System.Reflection.Metadata.MetadataUpdater.IsSupported=false
--feature:System.Resources.ResourceManager.AllowCustomResourceTypes=false
--feature:System.Resources.UseSystemResourceKeys=false
--feature:System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported=false
--feature:System.Runtime.InteropServices.BuiltInComInterop.IsSupported=false
--feature:System.Runtime.InteropServices.EnableConsumingManagedCodeFromNativeHosting=false
--feature:System.Runtime.InteropServices.EnableCppCLIHostActivation=false
--feature:System.Runtime.InteropServices.Marshalling.EnableGeneratedComInterfaceComImportInterop=false
--feature:System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization=false
--feature:System.StartupHookProvider.IsSupported=false
--feature:System.Text.Encoding.EnableUnsafeUTF7Encoding=false
--feature:System.Text.Json.JsonSerializer.IsReflectionEnabledByDefault=false
--feature:System.Threading.Thread.EnableAutoreleasePool=false
--feature:System.Threading.ThreadPool.UseWindowsThreadPool=true
--feature:System.Linq.Expressions.CanEmitObjectArrayDelegate=false
--runtimeknob:Microsoft.Extensions.DependencyInjection.VerifyOpenGenericServiceTrimmability=true
--runtimeknob:System.ComponentModel.TypeConverter.EnableUnsafeBinaryFormatterInDesigntimeLicenseContextSerialization=false
--runtimeknob:System.ComponentModel.TypeDescriptor.IsComObjectDescriptorSupported=false
--runtimeknob:System.Diagnostics.Tracing.EventSource.IsSupported=false
--runtimeknob:System.Globalization.Invariant=true
--runtimeknob:System.Globalization.PredefinedCulturesOnly=true
--runtimeknob:System.Reflection.Metadata.MetadataUpdater.IsSupported=false
--runtimeknob:System.Resources.ResourceManager.AllowCustomResourceTypes=false
--runtimeknob:System.Resources.UseSystemResourceKeys=false
--runtimeknob:System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported=false
--runtimeknob:System.Runtime.InteropServices.BuiltInComInterop.IsSupported=false
--runtimeknob:System.Runtime.InteropServices.EnableConsumingManagedCodeFromNativeHosting=false
--runtimeknob:System.Runtime.InteropServices.EnableCppCLIHostActivation=false
--runtimeknob:System.Runtime.InteropServices.Marshalling.EnableGeneratedComInterfaceComImportInterop=false
--runtimeknob:System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization=false
--runtimeknob:System.StartupHookProvider.IsSupported=false
--runtimeknob:System.Text.Encoding.EnableUnsafeUTF7Encoding=false
--runtimeknob:System.Text.Json.JsonSerializer.IsReflectionEnabledByDefault=false
--runtimeknob:System.Threading.Thread.EnableAutoreleasePool=false
--runtimeknob:System.Threading.ThreadPool.UseWindowsThreadPool=true
--runtimeknob:System.Linq.Expressions.CanEmitObjectArrayDelegate=false");
    }

    private void SetRuntimeIdentifier()
    {
        _arglistBuilder.AppendLine($"--targetos:{_os}");
        _arglistBuilder.AppendLine($"--targetarch:{_architecture}");
        _arglistBuilder.AppendLine($"--runtimeknob:RUNTIME_IDENTIFIER={_config.Config.RuntimeIdentifier}");
    }

    private void AddAdditionalConfig()
    {
        _arglistBuilder.AppendLine($@"--export-dynamic-symbol:DotNetRuntimeDebugHeader,DATA
--initassembly:System.Private.CoreLib
--initassembly:System.Private.StackTraceMetadata
--initassembly:System.Private.TypeLoader
--initassembly:System.Private.Reflection.Execution
--directpinvoke:System.Globalization.Native
--directpinvoke:System.IO.Compression.Native
--directpinvokelist:""{Path.Combine(_config.RuntimeIndependentILCompilerPackageRootDirectory, "build", "WindowsAPIs.txt")}""
--dehydrate
-O
-g
--stacktracedata
--scanreflection
--nowarn:""1701;1702;IL2121;1701;1702""
--warnaserr:"";NU1605;SYSLIB0011""
--singlewarn
--resilient
--generateunmanagedentrypoints:System.Private.CoreLib
--feature:System.Diagnostics.Debugger.IsSupported=false
--feature:System.Threading.ThreadPool.UseWindowsThreadPool=true");
    }

    private void AddLinkerFileSpecificArguments()
    {
        _linkerArglistBuilder.AppendLine($"\"{_objectFile}\"");
        _linkerArglistBuilder.AppendLine($"/OUT:\"{Path.Combine(_outputDirectory, $"{_config.Config.AssemblyName}{(_os == "win" ? ".exe" : "")}")}\"");
        _linkerArglistBuilder.AppendLine($"/DEF:\"{_exportsFile}\"");
    }

    private void AddLinkerDefaultArguments(out string linkerPath)
    {
        string kitsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Windows Kits");
        string sdkPathV10 = Path.Combine(kitsPath, "10");
        string netSdkRootDir = Path.Combine(kitsPath, "NETFXSDK");

        string netLibPath = Directory.GetDirectories(Path.Combine(netSdkRootDir)).Last();
        string libPath = Directory.GetDirectories(Path.Combine(sdkPathV10, "lib")).Last();

        string rid = RuntimeInformation.RuntimeIdentifier;
        string os = rid.Split('-')[0];
        string platform = rid.Split('-')[1];

        if (os == "win" && (!Directory.Exists(libPath) || !Directory.Exists(netLibPath)))
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0069_WinSdkToolNotFound,
                "The Windows SDK or parts of it could not be found on this machine, which is required for linking object files generated by the AOT compiler on Windows systems. To finish the compilation, either install the Windows SDK or enable the 'KeepIntermediateFiles' setting to prevent these object files from being deleted and link them manually.",
                "dc");
        }

        string msvcRootPath = WinSdkHelper.GetDirectoryPath("msvc", "To enable AOT compilation, you need to enter the path to the root directory of the current installation of MSVC. This needs to be done only once. ");
        // example: C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Tools\MSVC\14.40.33807
        string msvcPath = Directory.GetDirectories(msvcRootPath).Last();

        string hostDir = platform;
        if (platform.StartsWith('x'))
            hostDir = $"Host{platform}";

        linkerPath = Path.Combine(msvcPath, "bin", hostDir, platform, "link.exe");

        _linkerArglistBuilder.AppendLine($@"/LIBPATH:""{Path.Combine(msvcPath, "ATLMFC", "lib", platform)}""
/LIBPATH:""{Path.Combine(msvcPath, "lib", platform)}""
/LIBPATH:""{Path.Combine(netLibPath, "lib", "um", platform)}""
/LIBPATH:""{Path.Combine(libPath, "ucrt", platform)}""
/LIBPATH:""{Path.Combine(libPath, "um", platform)}""
""{Path.Combine(_config.ILCompilerPackageRootDirectory, "sdk", "bootstrapper.obj")}""
""{Path.Combine(_config.ILCompilerPackageRootDirectory, "sdk", "Runtime.WorkstationGC.lib")}""
""{Path.Combine(_config.ILCompilerPackageRootDirectory, "sdk", "eventpipe-disabled.lib")}""
""{Path.Combine(_config.ILCompilerPackageRootDirectory, "sdk", "Runtime.VxsortEnabled.lib")}""
""{Path.Combine(_config.ILCompilerPackageRootDirectory, "sdk", "standalonegc-disabled.lib")}""
""{Path.Combine(_config.ILCompilerPackageRootDirectory, "sdk", "System.Globalization.Native.Aot.lib")}""
""{Path.Combine(_config.ILCompilerPackageRootDirectory, "sdk", "System.IO.Compression.Native.Aot.lib")}""
""advapi32.lib""
""bcrypt.lib""
""crypt32.lib""
""iphlpapi.lib""
""kernel32.lib""
""mswsock.lib""
""ncrypt.lib""
""normaliz.lib""
""ntdll.lib""
""ole32.lib""
""oleaut32.lib""
""secur32.lib""
""user32.lib""
""version.lib""
""ws2_32.lib""
/NOLOGO /MANIFEST:NO
/DEBUG
/INCREMENTAL:NO
/SUBSYSTEM:CONSOLE
/ENTRY:wmainCRTStartup /NOEXP /NOIMPLIB
/NATVIS:""{Path.Combine(_config.RuntimeIndependentILCompilerPackageRootDirectory, "build", "NativeAOT.natvis")}""
/STACK:1572864
/IGNORE:4104
/NODEFAULTLIB:libucrt.lib
/DEFAULTLIB:ucrt.lib
/OPT:REF
/OPT:ICF
");
    }
}