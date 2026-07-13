using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Mutagen.Bethesda;
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

        // Older Synthesis versions defaulted every targeting checkbox to on, which wrote out a
        // meta file listing releases spanning every game family.  No real patcher targets all of
        // Oblivion + Skyrim + Fallout4 at once, so treat that as "no opinion" and blank it here.
        // Consumers then fall back to the patcher's included libraries to decide its releases.
        // (Synthesis' TargetedReleasesFromListingRetriever applies the same rule at runtime; this
        // cleans up the published listing so it stops advertising bogus releases in the meantime.)
        if (IsLegacySelectAll(customization.TargetedReleases))
        {
            customization.TargetedReleases = Array.Empty<GameRelease>();
        }

        return customization;
    }

    private static bool IsLegacySelectAll(GameRelease[] targeted)
    {
        // Match on family coverage rather than an exact release set so releases added after a
        // meta file was written don't cause it to be missed.
        if (targeted.Length == 0) return false;
        var families = targeted.Select(x => x.ToCategory()).ToHashSet();
        return families.Contains(GameCategory.Oblivion)
            && families.Contains(GameCategory.Skyrim)
            && families.Contains(GameCategory.Fallout4);
    }
}