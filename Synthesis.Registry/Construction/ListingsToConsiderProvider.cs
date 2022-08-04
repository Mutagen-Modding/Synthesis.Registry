using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitHubDependents;
using Noggog.Tooling.WorkEngine;
using Synthesis.Registry.MutagenScraper.Args;
using Synthesis.Registry.MutagenScraper.Dto;
using Synthesis.Registry.MutagenScraper.Github;
using Synthesis.Registry.MutagenScraper.Listings;
using Synthesis.Registry.MutagenScraper.Listings.Specialized;

namespace Synthesis.Registry.MutagenScraper.Construction;

public class ListingsToConsiderProvider
{
    private readonly ExistingListingsProvider _existingListingsProvider;
    private readonly IWorkDropoff _workDropoff;
    private readonly HeadShaRetriever _headShaRetriever;
    private readonly IShouldShortCircuitOnShaProvider _shortCircuitOnShaProvider;
    private readonly IDependenciesToConsiderIterator _dependenciesToConsider;
    private readonly INumToProcessProvider _numToProcessProvider;

    public ListingsToConsiderProvider(
        ExistingListingsProvider existingListingsProvider,
        IWorkDropoff workDropoff, 
        IDependenciesToConsiderIterator dependenciesToConsider,
        HeadShaRetriever headShaRetriever,
        IShouldShortCircuitOnShaProvider shortCircuitOnShaProvider,
        INumToProcessProvider numToProcessProvider)
    {
        _existingListingsProvider = existingListingsProvider;
        _workDropoff = workDropoff;
        _dependenciesToConsider = dependenciesToConsider;
        _headShaRetriever = headShaRetriever;
        _shortCircuitOnShaProvider = shortCircuitOnShaProvider;
        _numToProcessProvider = numToProcessProvider;
    }

    public async Task<IReadOnlyList<InternalRepositoryListing>> Get()
    {
        var deps = await _dependenciesToConsider.Get().ToArrayAsync();

        var cancelSource = new CancellationTokenSource();
        
        List<Task<InternalRepositoryListing?>> retrievals = new();
        foreach (var dep in deps)
        {
            retrievals.Add(_workDropoff.EnqueueAndWait(
                () => ConvertAndFilterOnSha(dep, cancelSource.Token),
                cancelSource.Token));
        }
        
        var toProcessEnumer = retrievals.ToAsyncEnumerable()
            .SelectAwait(async x => await x)
            .Where(x => x != null)
            .Select(x => x!);
        if (_numToProcessProvider.NumToProcessPer.HasValue)
        {
            toProcessEnumer = toProcessEnumer
                .Take(_numToProcessProvider.NumToProcessPer.Value);
        }
        
        var ret = await toProcessEnumer.ToArrayAsync();
        
        cancelSource.Cancel();
        
        return ret;
    }

    private async Task<InternalRepositoryListing?> ConvertAndFilterOnSha(Dependent dep, CancellationToken cancel)
    {
        var sha = await _headShaRetriever.TryGetSha(dep, cancel);
        if (sha == null) return null!;
        
        cancel.ThrowIfCancellationRequested();

        var ret = new InternalRepositoryListing(
            AvatarURL: dep.AvatarURL,
            User: dep.User!,
            Repository: dep.Repository!,
            Sha: sha);

        if (_shortCircuitOnShaProvider.ShouldShortCircuit
            && _existingListingsProvider.RepositoryDictionary.Value.TryGetValue(ret.Key, out var existing)
            && existing.Sha == sha)
        {
            return null!;
        }

        return ret;
    }
}