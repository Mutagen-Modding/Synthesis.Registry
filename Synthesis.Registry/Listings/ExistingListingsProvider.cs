using System;
using System.Collections.Generic;
using System.Linq;
using Synthesis.Bethesda.DTO;

namespace Synthesis.Registry.MutagenScraper.Listings;

public class ExistingListingsProvider
{
    private readonly ScrapeListingsPathProvider _pathProvider;
    private readonly ListingsReader _reader;
        
    public Lazy<MutagenPatchersListing> Listings { get; }
    public Lazy<IReadOnlyDictionary<ListingKey, RepositoryListing>> RepositoryDictionary { get; }

    public ExistingListingsProvider(
        ScrapeListingsPathProvider pathProvider,
        ListingsReader reader)
    {
        _pathProvider = pathProvider;
        _reader = reader;
        Listings = new Lazy<MutagenPatchersListing>(() => _reader.Read(_pathProvider.Path));
        RepositoryDictionary = new Lazy<IReadOnlyDictionary<ListingKey, RepositoryListing>>(GetDict);
    }

    private IReadOnlyDictionary<ListingKey, RepositoryListing> GetDict()
    {
        return Listings.Value
            .Repositories.ToDictionary(x => new ListingKey(x.User, x.Repository), x => x);
    }
}