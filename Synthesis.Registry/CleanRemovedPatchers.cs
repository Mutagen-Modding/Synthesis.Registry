using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Noggog;
using Synthesis.Bethesda.DTO;
using Synthesis.Registry.MutagenScraper.Listings;

namespace Synthesis.Registry.MutagenScraper;

public class CleanRemovedPatchers
{
    private readonly ISynthesisDependentsProvider _dependentsProvider;

    public CleanRemovedPatchers(
        ISynthesisDependentsProvider dependentsProvider)
    {
        _dependentsProvider = dependentsProvider;
    }

    public async Task<MutagenPatchersListing> Clean(MutagenPatchersListing existingListings)
    {
        var listed = await _dependentsProvider.Get();
        var listedSet = listed
            .Select(x => new ListingKey(x.User!, x.Repository!))
            .ToHashSet();

        IReadOnlySet<ListingKey> removed;
        using (HttpClient client = new HttpClient())
        {
            
            var waitLock = new SemaphoreSlim(5); // max 5 concurrent requests
            removed = (await Task.WhenAll(existingListings.Repositories
                    .Select(x => new ListingKey(x.User, x.Repository))
                .Where(x => !listedSet.Contains(x))
                .Select(x => Task.Run(() => ReturnIfMissing(client, x, waitLock)))))
                .WhereNotNull()
                .ToHashSet();
        }

        return new MutagenPatchersListing()
        {
            Repositories = existingListings.Repositories
                .Where(x => !removed.Contains(new ListingKey(x.User, x.Repository)))
                .ToArray()
        };
    }

    private async Task<ListingKey?> ReturnIfMissing(HttpClient client, ListingKey listing, SemaphoreSlim waitLock)
    {
        await waitLock.WaitAsync();
        
        var url = $"https://github.com/{listing.User}/{listing.Repository}";
        
        HttpResponseMessage response = await client.SendAsync(
            new HttpRequestMessage(HttpMethod.Head, url)
        );

        // Don't spam
        await Task.Delay(250);

        if (response.StatusCode != HttpStatusCode.NotFound)
        {
            return null;
        }
        
        Console.WriteLine("Removing missing patcher: " + url);
        return listing;
    }
}