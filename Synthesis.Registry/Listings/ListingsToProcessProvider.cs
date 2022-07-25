using System;
using System.Collections.Generic;
using System.Linq;
using Synthesis.Registry.MutagenScraper.Dto;

namespace Synthesis.Registry.MutagenScraper.Listings
{
    public class ListingsToProcessProvider
    {
        public IArgProvider ArgProvider { get; }
        public ISynthesisDependentsProvider DependentsProvider { get; }

        public ListingsToProcessProvider(
            IArgProvider argProvider,
            ISynthesisDependentsProvider dependentsProvider)
        {
            ArgProvider = argProvider;
            DependentsProvider = dependentsProvider;
        }
        
        public async IAsyncEnumerable<Listing> Get()
        {
            var listings = await DependentsProvider.Get();
            listings = listings.Where(x => x.User != null && x.Repository != null).ToArray();
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
                yield return new Listing(listing.AvatarURL, listing.User!, listing.Repository!);
            }
        }
    }
}