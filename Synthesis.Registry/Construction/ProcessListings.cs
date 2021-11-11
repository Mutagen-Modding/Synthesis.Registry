using System;
using System.Collections.Generic;
using System.Linq;
using Synthesis.Bethesda.DTO;
using Synthesis.Registry.MutagenScraper.Listings;

namespace Synthesis.Registry.MutagenScraper.Construction
{
    public class GetRepositoryListingsToConsider
    {
        private readonly ExistingListingsProvider _existingListingsProvider;
        private readonly ConstructRepositoryListings _constructRepositoryListings;
        private readonly ListingsToProcessProvider _listingsToProcess;

        public GetRepositoryListingsToConsider(
            ExistingListingsProvider existingListingsProvider,
            ConstructRepositoryListings constructRepositoryListings,
            ListingsToProcessProvider listingsToProcess)
        {
            _existingListingsProvider = existingListingsProvider;
            _constructRepositoryListings = constructRepositoryListings;
            _listingsToProcess = listingsToProcess;
        }
        
        public IAsyncEnumerable<RepositoryListing> Get()
        {
            var dict = _existingListingsProvider.Listings.Value
                .Repositories.ToDictionary(x => new ListingKey(x.User, x.Repository), x => x);
            
            return _listingsToProcess.Get()
                .SelectAwait(async (dep) =>
                {
                    Console.WriteLine($"Processing {dep}");
                    return await _constructRepositoryListings.Construct(dep, dict.GetValueOrDefault(new ListingKey(dep.User, dep.Repository)));
                })
                .Where(r =>
                {
                    if (r.Patchers.Length == 0)
                    {
                        System.Console.WriteLine($"{r.User}/{r.Repository} skipped because it had no listed patchers.");
                        return false;
                    }

                    return true;
                });
        }
    }
}