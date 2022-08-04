using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Synthesis.Bethesda;
using Synthesis.Bethesda.DTO;
using Synthesis.Registry.MutagenScraper.Dto;
using Synthesis.Registry.MutagenScraper.Github;

namespace Synthesis.Registry.MutagenScraper.Construction;

public class PatcherCustomizationReader
{
    private readonly GithubContentDownloader _contentDownloader;
    private readonly JsonSerializerOptionsProvider _jsonOptions;

    public PatcherCustomizationReader(
        GithubContentDownloader contentDownloader,
        JsonSerializerOptionsProvider jsonOptions)
    {
        _contentDownloader = contentDownloader;
        _jsonOptions = jsonOptions;
    }

    public async Task<PatcherCustomization?> GetCustomization(InternalRepositoryListing dep, string proj)
    {
        var metaPath = Path.Combine(Path.GetDirectoryName(proj)!, Constants.MetaFileName);
        var content = await _contentDownloader.TryGetContent(dep, metaPath);
        if (content == null) return null;
        var customization = JsonSerializer.Deserialize<PatcherCustomization>(content, _jsonOptions.Options)!;
        if (string.IsNullOrWhiteSpace(customization.Nickname))
        {
            customization.Nickname = $"{dep.User}/{dep.Repository}";
        }

        // Backwards compatibility
        try
        {
            using var doc = JsonDocument.Parse(content);
            foreach (var elem in doc.RootElement.EnumerateObject())
            {
                if (elem.NameEquals("HideByDefault")
                    && elem.Value.GetBoolean())
                {
                    customization.Visibility = VisibilityOptions.IncludeButHide;
                }
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"{proj} Error handling backwards compatibility: {ex}");
        }
        
        return customization;
    }
}