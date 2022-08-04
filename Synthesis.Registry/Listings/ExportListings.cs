using System;
using System.IO.Abstractions;
using System.Text.Json;
using Synthesis.Bethesda.DTO;

namespace Synthesis.Registry.MutagenScraper.Listings
{
    public class ExportListings
    {
        private readonly IFileSystem _fileSystem;
        private readonly ScrapeListingsPathProvider _pathProvider;
        private readonly JsonSerializerOptionsProvider _jsonOptions;

        public ExportListings(
            IFileSystem fileSystem,
            ScrapeListingsPathProvider pathProvider,
            JsonSerializerOptionsProvider jsonOptions)
        {
            _fileSystem = fileSystem;
            _pathProvider = pathProvider;
            _jsonOptions = jsonOptions;
        }
        
        public void Write(MutagenPatchersListing listings)
        {
            var txt = JsonSerializer.Serialize(
                listings,
                _jsonOptions.Options);
            
            _fileSystem.File.WriteAllText(_pathProvider.Path, txt);

            Console.WriteLine($"{_pathProvider.Path} {(_fileSystem.File.Exists(_pathProvider.Path) ? "exists." : "does not exist!")}");
        }
    }
}