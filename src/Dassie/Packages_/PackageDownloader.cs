using Newtonsoft.Json.Linq;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
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
    /// <summary>
    /// Downloads the specified package.
    /// </summary>
    /// <param name="packageId">The ID of the package.</param>
    /// <param name="version">The package version to download. If <see langword="null"/>, the latest version is downloaded.</param>
    /// <returns>The downloaded version string.</returns>
    public static string DownloadPackage(string packageId, string version = null)
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
        WriteLine($"Downloading package '{packageId}' ({FormatBytes(bytes)}).");

        using MemoryStream ms = new();

        package.CopyNupkgToStreamAsync(
            packageId,
            targetVersion,
            ms,
            cache,
            NullLogger.Instance,
            CancellationToken.None).Wait();

        using PackageArchiveReader reader = new(ms);
        NuspecReader nuspecReader = reader.GetNuspecReaderAsync(CancellationToken.None).Result;

        ZipFile.ExtractToDirectory(ms, packageDir);
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