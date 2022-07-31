using System;
using System.Collections.Generic;
using System.Linq;
using Synthesis.Registry.MutagenScraper.Dto;
using Synthesis.Registry.MutagenScraper.Github;

namespace Synthesis.Registry.MutagenScraper.Listings
{
    public class ListingsToProcessProvider
    {
        public RunScraperCommand ArgProvider { get; }
        public ISynthesisDependentsProvider DependentsProvider { get; }
        public IHeadShaRetriever HeadShaRetriever { get; }

        public ListingsToProcessProvider(
            RunScraperCommand argProvider,
            ISynthesisDependentsProvider dependentsProvider, 
            IHeadShaRetriever headShaRetriever)
        {
            ArgProvider = argProvider;
            DependentsProvider = dependentsProvider;
            HeadShaRetriever = headShaRetriever;
        }
        
        public async IAsyncEnumerable<Listing> Get()
        {
            var dependents = await DependentsProvider.Get();
            
            dependents = dependents.Where(x => x.User != null && x.Repository != null).ToArray();
            
            var number = ArgProvider.RunNumber;
            var numToProcess = ArgProvider.NumToProcessPer ?? 20;
            var slots = (int)Math.Ceiling(1.0d * dependents.Count / numToProcess);
            var slot = number % slots;
            var toProcess = dependents
                .Skip(slot * numToProcess)
                .Take(numToProcess)
                .ToArray();
            Console.WriteLine($"Listings to process this run:");
            foreach (var dep in toProcess)
            {
                Console.WriteLine($"  {dep}");
            }
            foreach (var dep in toProcess)
            {
                var sha = await HeadShaRetriever.TryGetSha(dep);
                if (sha == null) continue;
                    
                yield return new Listing(
                    AvatarURL: dep.AvatarURL, 
                    User: dep.User!,
                    Repository: dep.Repository!,
                    Sha: sha);
            }
        }
    }
}