using System;
using System.IO.Abstractions;
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

        public ExistingListingsProvider(
            IFileSystem fileSystem,
            ScrapeListingsPathProvider pathProvider,
            JsonSerializerOptionsProvider jsonOptions)
        {
            _fileSystem = fileSystem;
            _pathProvider = pathProvider;
            _jsonOptions = jsonOptions;
            Listings = new Lazy<MutagenPatchersListing>(Read);
        }

        private MutagenPatchersListing Read()
        {
            if (!_fileSystem.File.Exists(_pathProvider.Path)) return new MutagenPatchersListing();

            return JsonSerializer.Deserialize<MutagenPatchersListing>(
                _fileSystem.File.ReadAllText(_pathProvider.Path),
                _jsonOptions.Options)!;
        }
    }
}