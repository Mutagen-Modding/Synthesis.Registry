using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using GitHubDependents;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins.Meta;
using Noggog;
using NuGet.Versioning;
using Synthesis.Bethesda.DTO;
using Synthesis.Registry.MutagenScraper.Github;

namespace Synthesis.Registry.MutagenScraper.Construction;

public class TargetReleasesLocator
{
    private readonly ListedCategoryRetriever _listedCategoryRetriever;

    public TargetReleasesLocator(ListedCategoryRetriever listedCategoryRetriever)
    {
        _listedCategoryRetriever = listedCategoryRetriever;
    }

    public GameRelease[] GetTargetReleases(string? projectContent, PatcherCustomization customization)
    {
        if (customization.TargetedReleases.Length > 0)
        {
            return customization.TargetedReleases;
        }
        
        if (projectContent == null) return Array.Empty<GameRelease>();

        var categories = _listedCategoryRetriever.GetListedCategories(projectContent);
        return categories
            .SelectMany(c => c.GetRelatedReleases())
            .Distinct()
            .ToArray();
    }
}