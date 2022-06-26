using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using GitHubDependents;
using Noggog;
using Octokit;
using Synthesis.Bethesda;
using Synthesis.Bethesda.DTO;
using Synthesis.Registry.MutagenScraper.Github;

namespace Synthesis.Registry.MutagenScraper.Construction
{
    public class ConstructListings
    {
        private readonly PatcherCustomizationRetriever _customizationRetriever;
        private readonly GithubContentDownloader _contentDownloader;
        private readonly ListedCategoryRetriever _categoryRetriever;
        private readonly TargetReleasesLocator _targetReleasesLocator;

        public ConstructListings(
            PatcherCustomizationRetriever customizationRetriever,
            GithubContentDownloader contentDownloader,
            ListedCategoryRetriever categoryRetriever,
            TargetReleasesLocator targetReleasesLocator)
        {
            _customizationRetriever = customizationRetriever;
            _contentDownloader = contentDownloader;
            _categoryRetriever = categoryRetriever;
            _targetReleasesLocator = targetReleasesLocator;
        }
        
        public async Task<PatcherListing[]> Construct(Dependent dep, IEnumerable<string> projs)
        {
            return (await projs
                .ToAsyncEnumerable()
                .SelectAwait(async proj =>
                {
                    var listing = new PatcherListing()
                    {
                        ProjectPath = proj,
                    };
                    try
                    {
                        listing.Customization = await _customizationRetriever.GetCustomization(dep, proj);
                        if (listing.Customization == null) return null;
                        var projContent = await _contentDownloader.TryGetContent(dep, proj);
                        if (projContent == null) return null;
                        listing.IncludedLibraries = _categoryRetriever.GetListedCategories(projContent);
                        listing.TargetedReleases = _targetReleasesLocator.GetTargetReleases(projContent, listing.Customization);
                    }
                    catch (Octokit.NotFoundException)
                    {
                    }
                    catch (Octokit.ApiException)
                    {
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"{proj} Error constructing listing: {ex}");
                        return null;
                    }
                    await Task.Delay(500);
                    return listing;
                })
                .ToListAsync())
                .NotNull()
                .Where(listing =>
                {
                    if (listing.Customization?.Visibility == VisibilityOptions.Exclude)
                    {
                        System.Console.WriteLine($"{dep} excluding {listing.ProjectPath}");
                        return false;
                    }
                    return true;
                })
                .ToArray();
        }
    
    }
}