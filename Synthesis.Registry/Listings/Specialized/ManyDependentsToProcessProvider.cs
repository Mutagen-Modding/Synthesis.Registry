using System;
using System.Collections.Generic;
using System.Linq;
using GitHubDependents;
using Synthesis.Registry.MutagenScraper.Args;

namespace Synthesis.Registry.MutagenScraper.Listings.Specialized
{
    public class ManyDependentsToProcessProvider : IDependenciesToConsiderIterator
    {
        public IRunScraperCommand ArgProvider { get; }
        public ISynthesisDependentsProvider DependentsProvider { get; }
        public ManyDependentsToProcessProvider(
            IRunScraperCommand argProvider,
            ISynthesisDependentsProvider dependentsProvider)
        {
            ArgProvider = argProvider;
            DependentsProvider = dependentsProvider;
        }

        public async IAsyncEnumerable<Dependent> Get()
        {
            var dependents = await DependentsProvider.Get();
            
            var number = ArgProvider.RunNumber;
            var numToProcess = ArgProvider.NumToProcessPer ?? int.MaxValue;
            var slots = (int)Math.Ceiling(1.0d * dependents.Count / numToProcess);
            var slot = number % slots;
            foreach (var dep in Iterate(dependents, slot * numToProcess))
            {
                yield return dep;
            }
        }

        private IEnumerable<Dependent> Iterate(IReadOnlyList<Dependent> deps, int startingWith)
        {
            for (int i = startingWith; i < deps.Count; i++)
            {
                yield return deps[i];
            }

            for (int i = 0; i < startingWith; i++)
            {
                yield return deps[i];
            }
        }
    }
}