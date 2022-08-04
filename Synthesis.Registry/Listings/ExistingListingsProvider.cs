using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;
using Synthesis.Bethesda.DTO;

namespace Synthesis.Registry.MutagenScraper.Listings
{
    public class ExistingListingsProvider
    {
        private readonly IFileSystem _fileSystem;
        private readonly ScrapeListingsPathProvider _pathProvider;
        private readonly JsonSerializerOptionsProvider _jsonOptions;
        
        public Lazy<MutagenPatchersListing> Listings { get; }
        public Lazy<IReadOnlyDictionary<ListingKey, RepositoryListing>> RepositoryDictionary { get; }

        public ExistingListingsProvider(
            IFileSystem fileSystem,
            ScrapeListingsPathProvider pathProvider,
            JsonSerializerOptionsProvider jsonOptions)
        {
            _fileSystem = fileSystem;
            _pathProvider = pathProvider;
            _jsonOptions = jsonOptions;
            Listings = new Lazy<MutagenPatchersListing>(Read);
            RepositoryDictionary = new Lazy<IReadOnlyDictionary<ListingKey, RepositoryListing>>(GetDict);
        }

        private MutagenPatchersListing Read()
        {
            if (!_fileSystem.File.Exists(_pathProvider.Path)) return new MutagenPatchersListing();

            return JsonSerializer.Deserialize<MutagenPatchersListing>(
                _fileSystem.File.ReadAllText(_pathProvider.Path),
                _jsonOptions.Options)!;
        }

        private IReadOnlyDictionary<ListingKey, RepositoryListing> GetDict()
        {
            return Listings.Value
                .Repositories.ToDictionary(x => new ListingKey(x.User, x.Repository), x => x);
        }
    }
}