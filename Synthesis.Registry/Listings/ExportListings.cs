using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;
using Synthesis.Bethesda.DTO;

namespace Synthesis.Registry.MutagenScraper.Listings;

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

    public void Write(IEnumerable<RepositoryListing> listings)
    {
        Write(new MutagenPatchersListing()
        {
            Repositories = listings
                .OrderBy(x => x.Repository)
                .ToArray()
        });
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