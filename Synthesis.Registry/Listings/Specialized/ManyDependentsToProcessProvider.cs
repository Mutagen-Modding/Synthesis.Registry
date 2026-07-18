using System;
using System.Collections.Generic;
using System.Linq;
using GitHubDependents;
using Synthesis.Registry.MutagenScraper.Args;
using Synthesis.Registry.MutagenScraper.Listings;

namespace Synthesis.Registry.MutagenScraper.Listings.Specialized;

public class ManyDependentsToProcessProvider : IDependenciesToConsiderIterator
{
    public IRunScraperCommand ArgProvider { get; }
    public ISynthesisDependentsProvider DependentsProvider { get; }
    public ExistingListingsProvider ExistingListingsProvider { get; }
    public ManyDependentsToProcessProvider(
        IRunScraperCommand argProvider,
        ISynthesisDependentsProvider dependentsProvider,
        ExistingListingsProvider existingListingsProvider)
    {
        ArgProvider = argProvider;
        DependentsProvider = dependentsProvider;
        ExistingListingsProvider = existingListingsProvider;
    }

    public async IAsyncEnumerable<Dependent> Get()
    {
        var dependents = (await DependentsProvider.Get()).ToList();

        // Fold saved listings dropped from the live feed into the rotation so they still get re-scraped.
        var seen = dependents
            .Select(x => (x.User!.ToLowerInvariant(), x.Repository!.ToLowerInvariant()))
            .ToHashSet();
        foreach (var existing in ExistingListingsProvider.Listings.Value.Repositories)
        {
            if (!seen.Add((existing.User.ToLowerInvariant(), existing.Repository.ToLowerInvariant()))) continue;
            dependents.Add(new Dependent
            {
                User = existing.User,
                Repository = existing.Repository,
                AvatarURL = existing.AvatarURL,
            });
        }

        var number = ArgProvider.RunNumber ?? 0;
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