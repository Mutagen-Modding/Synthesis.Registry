using System.IO.Abstractions;
using System.Text.Json;
using Noggog;
using Synthesis.Bethesda.DTO;

namespace Synthesis.Registry.MutagenScraper.Listings;

public class ListingsReader
{
    private readonly IFileSystem _fileSystem;
    private readonly JsonSerializerOptionsProvider _jsonOptions;


    public ListingsReader(
        IFileSystem fileSystem,
        JsonSerializerOptionsProvider jsonOptions)
    {
        _fileSystem = fileSystem;
        _jsonOptions = jsonOptions;
    }

    public MutagenPatchersListing Read(FilePath listingsPath)
    {
        if (!_fileSystem.File.Exists(listingsPath)) return new MutagenPatchersListing();

        return JsonSerializer.Deserialize<MutagenPatchersListing>(
            _fileSystem.File.ReadAllText(listingsPath),
            _jsonOptions.Options)!;
    }
}