using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Noggog.WorkEngine;
using Synthesis.Bethesda.DTO;
using Synthesis.Registry.MutagenScraper.Listings;

namespace Synthesis.Registry.MutagenScraper.Construction;

public class GetRepositoryListingsToUpdate
{
    private readonly IWorkDropoff _workDropoff;
    private readonly ExistingListingsProvider _existingListingsProvider;
    private readonly ConstructRepositoryListing _constructRepositoryListing;
    private readonly ListingsToConsiderProvider _listingsToConsiderProvider;

    public GetRepositoryListingsToUpdate(
        IWorkDropoff workDropoff,
        ExistingListingsProvider existingListingsProvider,
        ConstructRepositoryListing constructRepositoryListing,
        ListingsToConsiderProvider listingsToConsiderProvider)
    {
        _workDropoff = workDropoff;
        _existingListingsProvider = existingListingsProvider;
        _constructRepositoryListing = constructRepositoryListing;
        _listingsToConsiderProvider = listingsToConsiderProvider;
    }
        
    public async Task<RepositoryListing[]> Get()
    {
        var toProcess = await _listingsToConsiderProvider.Get();
            
        foreach (var dep in toProcess)
        {
            Console.WriteLine($"  {dep}");
        }

        return await _workDropoff.EnqueueAndWait(toProcess, 
            async (dep) =>
            {
                Console.WriteLine($"Processing {dep}");
                return await _constructRepositoryListing.Construct(dep,
                    _existingListingsProvider.RepositoryDictionary.Value.GetValueOrDefault(new ListingKey(dep.User,
                        dep.Repository)));
            });
    }
}