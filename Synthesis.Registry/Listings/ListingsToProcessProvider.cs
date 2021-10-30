using System;
using System.Collections.Generic;
using System.Linq;
using GitHubDependents;

namespace Synthesis.Registry.MutagenScraper.Listings
{
    public class ListingsToProcessProvider
    {
        public IArgProvider ArgProvider { get; }
        public ISynthesisListingsProvider ListingsProvider { get; }

        public ListingsToProcessProvider(
            IArgProvider argProvider,
            ISynthesisListingsProvider listingsProvider)
        {
            ArgProvider = argProvider;
            ListingsProvider = listingsProvider;
        }
        
        public async IAsyncEnumerable<Dependent> Get()
        {
            var listings = await ListingsProvider.Get();
            var number = ArgProvider.RunNumber;
            var slots = (int)Math.Ceiling(1.0d * listings.Count / ArgProvider.NumToProcessPer);
            var slot = number % slots;
            var toProcess = listings
                .Skip(slot * ArgProvider.NumToProcessPer)
                .Take(ArgProvider.NumToProcessPer)
                .ToArray();
            Console.WriteLine($"Listings to process this run:");
            foreach (var listing in toProcess)
            {
                Console.WriteLine($"  {listing}");
            }
            foreach (var listing in toProcess)
            {
                yield return listing;
            }
        }
    }
}