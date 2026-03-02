using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dassie.Extensions.Web;

static class ExtensionDownloader
{
    private static readonly HttpClient _client = new();
    public static string Source { get; set; } = "https://losch.at/dassie";

    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<List<ExtensionMetadata>> GetPackagesAsync(string extensionId)
    {
        List<ExtensionMetadata> packages = [];

        try
        {
            packages.AddRange(await FetchAsync(extensionId));
        }
        catch (Exception ex)
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0226_RemoteExtensionException,
                nameof(StringHelper.ExtensionDownloader_FetchError), [extensionId, ex.Message],
                CompilerExecutableName);
        }

        return packages;
    }

    private static async Task<List<ExtensionMetadata>> FetchAsync(string extensionId, string apiKey = null)
    {
        HttpRequestMessage request = new(HttpMethod.Get, $"{Source}/api/extensions/{extensionId}");

        if (!string.IsNullOrEmpty(apiKey))
            request.Headers.Add("Authorization", $"Bearer {apiKey}");

        HttpResponseMessage response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();
        List<ExtensionMetadata> extensions = JsonSerializer.Deserialize<List<ExtensionMetadata>>(json, _options);
        return extensions ?? [];
    }

    public static async Task<(byte[] Bytes, string FileName)> DownloadExtension(ExtensionMetadata metadata)
    {
        byte[] assemblyBytes = null;

        if (!string.IsNullOrEmpty(metadata.Uri) && File.Exists(metadata.Uri))
            assemblyBytes = File.ReadAllBytes(metadata.Uri);
        else
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync(metadata.Uri);
                response.EnsureSuccessStatusCode();
                assemblyBytes = await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0226_RemoteExtensionException,
                    nameof(StringHelper.ExtensionDownloader_DownloadError), [metadata.Metadata.Name, metadata.Uri, ex.Message],
                    CompilerExecutableName);
            }
        }

        return (assemblyBytes, metadata.Uri.Split('/').Last());
    }
}