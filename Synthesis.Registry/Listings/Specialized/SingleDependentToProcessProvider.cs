using System;
using System.Collections.Generic;
using GitHubDependents;
using Synthesis.Registry.MutagenScraper.Args;

namespace Synthesis.Registry.MutagenScraper.Listings.Specialized;

public class SingleDependentToProcessProvider : IDependenciesToConsiderIterator
{
    private readonly RunSingleScrapeCommand _runSingleScrapeCommand;
    private readonly ISynthesisDependentsProvider _dependentsProvider;

    public SingleDependentToProcessProvider(
        RunSingleScrapeCommand runSingleScrapeCommand, 
        ISynthesisDependentsProvider dependentsProvider)
    {
        _runSingleScrapeCommand = runSingleScrapeCommand;
        _dependentsProvider = dependentsProvider;
    }

    public async IAsyncEnumerable<Dependent> Get()
    {
        var dependents = await _dependentsProvider.Get();
        foreach (var dependent in dependents)
        {
            if (!string.Equals(dependent.User, _runSingleScrapeCommand.User, StringComparison.OrdinalIgnoreCase)) continue;

            if (!string.Equals(dependent.Repository, _runSingleScrapeCommand.Repository, StringComparison.OrdinalIgnoreCase)) continue;

            yield return dependent;
            yield break;
        }
    }
}