using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Noggog;
using Synthesis.Bethesda.DTO;
using Synthesis.Registry.MutagenScraper.Dto;
using Synthesis.Registry.MutagenScraper.Listings;

namespace Synthesis.Registry.MutagenScraper.Construction
{
    public class ConstructListings
    {
        private readonly PatcherCustomizationRetriever _customizationRetriever;

        public ConstructListings(
            PatcherCustomizationRetriever customizationRetriever)
        {
            _customizationRetriever = customizationRetriever;
        }
        
        public async Task<PatcherListing[]> Construct(Listing dep, IEnumerable<string> projs)
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