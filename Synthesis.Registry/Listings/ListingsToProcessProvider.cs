using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using Noggog.Utility;
using Synthesis.Registry.MutagenScraper.Dto;
using Synthesis.Registry.MutagenScraper.Github;

namespace Synthesis.Registry.MutagenScraper.Listings
{
    public class ListingsToProcessProvider
    {
        private readonly IProcessFactory _processFactory;
        public IArgProvider ArgProvider { get; }
        public ISynthesisDependentsProvider DependentsProvider { get; }
        public IHeadShaRetriever HeadShaRetriever { get; }

        public ListingsToProcessProvider(
            IArgProvider argProvider,
            IProcessFactory processFactory,
            ISynthesisDependentsProvider dependentsProvider, 
            IHeadShaRetriever headShaRetriever)
        {
            _processFactory = processFactory;
            ArgProvider = argProvider;
            DependentsProvider = dependentsProvider;
            HeadShaRetriever = headShaRetriever;
        }
        
        public async IAsyncEnumerable<Listing> Get()
        {
            var dependents = await DependentsProvider.Get();
            
            dependents = dependents.Where(x => x.User != null && x.Repository != null).ToArray();
            
            var number = ArgProvider.RunNumber;
            var slots = (int)Math.Ceiling(1.0d * dependents.Count / ArgProvider.NumToProcessPer);
            var slot = number % slots;
            var toProcess = dependents
                .Skip(slot * ArgProvider.NumToProcessPer)
                .Take(ArgProvider.NumToProcessPer)
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