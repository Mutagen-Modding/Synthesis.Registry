using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Noggog;
using Noggog.Tooling.WorkEngine;
using Synthesis.Bethesda.DTO;
using Synthesis.Registry.MutagenScraper.Dto;
using Synthesis.Registry.MutagenScraper.Processing;

namespace Synthesis.Registry.MutagenScraper.Construction
{
    public class ConstructListings
    {
        private readonly PatcherCustomizationReader _customizationReader;
        private readonly IWorkDropoff _dropoff;
        private readonly IEnumerable<IPatcherListingProcessor> _subProcessors;

        public ConstructListings(
            PatcherCustomizationReader customizationReader,
            IWorkDropoff dropoff,
            IEnumerable<IPatcherListingProcessor> subProcessors)
        {
            _customizationReader = customizationReader;
            _dropoff = dropoff;
            _subProcessors = subProcessors;
        }
        
        public async Task<PatcherListing[]> Construct(InternalRepositoryListing dep, IEnumerable<string> projs)
        {
            var ret = await _dropoff.EnqueueAndWait(projs,
                async proj =>
                {
                    var listing = new PatcherListing()
                    {
                        ProjectPath = proj,
                    };
                    try
                    {
                        listing.Customization = await _customizationReader.GetCustomization(dep, proj);
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"{proj} Error constructing listing: {ex}");
                        return null;
                    }
                    if (listing.Customization?.Visibility == VisibilityOptions.Exclude)
                    {
                        System.Console.WriteLine($"{dep} excluding {listing.ProjectPath}");
                        return null;
                    }

                    await _dropoff.EnqueueAndWait(_subProcessors, (sp) => sp.Process(dep, listing));
                    return listing;
                });
            return ret.NotNull().ToArray();
        }
    
    }
}