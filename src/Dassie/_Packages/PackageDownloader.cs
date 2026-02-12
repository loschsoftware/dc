using Newtonsoft.Json.Linq;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol.Plugins;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dassie.Packages;

/// <summary>
/// Provides functionality for downloading NuGet packages.
/// </summary>
internal static class PackageDownloader
{
    private class ProgressStream : Stream
    {
        private readonly Stream _innerStream;
        private readonly long _totalBytes;
        private readonly IProgress<double> _progress;
        private long _bytesWritten;

        public ProgressStream(Stream innerStream, long totalBytes, IProgress<double> progress)
        {
            _innerStream = innerStream;
            _totalBytes = totalBytes;
            _progress = progress;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _innerStream.Write(buffer, offset, count);
            _bytesWritten += count;
            _progress?.Report((double)_bytesWritten / _totalBytes * 100);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
            _bytesWritten += count;
            _progress?.Report((double)_bytesWritten / _totalBytes * 100);
        }

        public override bool CanRead => _innerStream.CanRead;
        public override bool CanSeek => _innerStream.CanSeek;
        public override bool CanWrite => _innerStream.CanWrite;
        public override long Length => _innerStream.Length;
        public override long Position
        {
            get => _innerStream.Position;
            set => _innerStream.Position = value;
        }
        public override void Flush() => _innerStream.Flush();
        public override int Read(byte[] buffer, int offset, int count)
            => _innerStream.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin)
            => _innerStream.Seek(offset, origin);
        public override void SetLength(long value)
            => _innerStream.SetLength(value);
    }

    /// <summary>
    /// Downloads the specified package.
    /// </summary>
    /// <param name="packageId">The ID of the package.</param>
    /// <param name="version">The package version to download. If <see langword="null"/>, the latest version is downloaded.</param>
    /// <param name="isDependency">Wheter or not the package is downloaded as a dependency of another package.</param>
    /// <returns>The downloaded version string.</returns>
    public static string DownloadPackage(string packageId, string version = null, bool isDependency = false)
    {
        SourceCacheContext cache = new();
        SourceRepository repo = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        FindPackageByIdResource package = repo.GetResource<FindPackageByIdResource>();

        IEnumerable<NuGetVersion> versions = package.GetAllVersionsAsync(
            packageId,
            cache,
            NullLogger.Instance,
            CancellationToken.None).Result
            .Where(v => !v.IsPrerelease);

        if (!versions.Any())
        {
            string dir = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Packages", packageId)).FullName;
            string[] subDirs = Directory.GetDirectories(dir);

            if (subDirs.Length == 0)
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0104_NetworkError,
                    $"Could not download package '{packageId}'.",
                    CompilerExecutableName);

                return "";
            }

            return subDirs.Last().Split(Path.DirectorySeparatorChar).Last();
        }

        NuGetVersion targetVersion = versions.Last();

        if (!string.IsNullOrEmpty(version))
        {
            if (versions.Any(v => v.ToFullString() == version))
                targetVersion = versions.First(v => v.ToFullString() == version);

            else
            {
                EmitWarningMessage(
                    0, 0, 0,
                    DS0105_InvalidPackageReference,
                    $"Version '{version}' not found in package '{packageId}'. Using latest version instead, which is '{targetVersion.ToFullString()}'.",
                    CompilerExecutableName);
            }
        }

        string packageDir = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Packages", packageId, targetVersion.ToFullString())).FullName;
        if (File.Exists(Path.Combine(packageDir, "[Content_Types].xml"))) // Just check if any file belonging to the package exists
            return targetVersion.ToFullString();

        long bytes = GetPackageSizeFromCatalog(new(packageId, targetVersion), cache, repo, CancellationToken.None).Result.Value;

        Progress<double> progress = new(percent =>
        {
            WriteString($"\r{(isDependency ? "  - " : "")}Downloading package '{packageId}' ({FormatBytes(bytes)}). [{percent:000.00}%]");
        });

        using MemoryStream ms = new();
        using ProgressStream ps = new(ms, bytes, progress);

        package.CopyNupkgToStreamAsync(
            packageId,
            targetVersion,
            ps,
            cache,
            NullLogger.Instance,
            CancellationToken.None).Wait();

        using PackageArchiveReader reader = new(ms);
        NuspecReader nuspecReader = reader.GetNuspecReaderAsync(CancellationToken.None).Result;
        ZipFile.ExtractToDirectory(ms, packageDir);

        WriteLine("");

        // TODO: Also install package dependencies and move them into the build directory
        //foreach (PackageDependency dependency in nuspecReader.GetDependencyGroups().SelectMany(p => p.Packages))
        //    DownloadPackage(dependency.Id, dependency.VersionRange.MaxVersion?.ToString(), true);

        return targetVersion.ToFullString();
    }

    private static async Task<long?> GetPackageSizeFromCatalog(PackageIdentity packageIdentity, SourceCacheContext cache, SourceRepository sourceRepository, CancellationToken token)
    {
        RegistrationResourceV3 registrationResource = await sourceRepository.GetResourceAsync<RegistrationResourceV3>();
        JObject packageMetadata = await registrationResource.GetPackageMetadata(packageIdentity, cache, NullLogger.Instance, token);
        string catalogItemUrl = packageMetadata.Value<string>("@id");

        HttpSourceResource sourceResource = await sourceRepository.GetResourceAsync<HttpSourceResource>();
        JObject catalogItem = await sourceResource.HttpSource.GetJObjectAsync(
            new HttpSourceRequest(catalogItemUrl, NullLogger.Instance),
            NullLogger.Instance,
            token);

        return catalogItem.Value<long>("packageSize");
    }

    private static string FormatBytes(long byteCount)
    {
        (double value, string suffix) = (double)byteCount switch
        {
            < 1e3 => (byteCount, "B"),
            < 1e6 => (byteCount / 1e3, "KB"),
            < 1e9 => (byteCount / 1e6, "MB"),
            < 1e12 => (byteCount / 1e9, "GB"),
            _ => (byteCount / 1e12, "TB")
        };

        return $"{value.ToString("#.##", CultureInfo.InvariantCulture)} {suffix}";
    }
}