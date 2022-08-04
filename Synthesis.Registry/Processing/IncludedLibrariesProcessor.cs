using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Mutagen.Bethesda;
using Noggog;
using Noggog.DotNetCli.DI;
using Synthesis.Bethesda.DTO;
using Synthesis.Registry.MutagenScraper.Dto;
using Synthesis.Registry.MutagenScraper.Github;

namespace Synthesis.Registry.MutagenScraper.Processing;

public class IncludedLibrariesProcessor : IPatcherListingProcessor
{
    private readonly IFileSystem _fileSystem;
    private readonly IQueryNugetListing _queryNugetListing;
    private readonly IGetFolderClone _getFolderClone;
    private static readonly Dictionary<string, GameCategory> UsingsToCategories = new();

    static IncludedLibrariesProcessor()
    {
        UsingsToCategories = EnumExt<GameCategory>.Values
            .ToDictionary(x => $"using Mutagen.Bethesda.{x};", x => x);
    }

    public IncludedLibrariesProcessor(
        IFileSystem fileSystem,
        IQueryNugetListing queryNugetListing,
        IGetFolderClone getFolderClone)
    {
        _fileSystem = fileSystem;
        _queryNugetListing = queryNugetListing;
        _getFolderClone = getFolderClone;
    }

    public async Task Process(InternalRepositoryListing repositoryListing, PatcherListing listing)
    {
        listing.IncludedLibraries = await GetCategories(repositoryListing, listing);
        if (listing.IncludedLibraries.Length == 0)
        {
            int wer = 23;
            wer++;
        }
    }

    private async Task<GameCategory[]> GetCategories(InternalRepositoryListing repositoryListing, PatcherListing listing)
    {
        var repoFolder = _getFolderClone.Get(repositoryListing);
        var projPath = Path.Combine(repoFolder, listing.ProjectPath);
        
        var categories = await GetCategoriesFromNuget(projPath);
        
        if (categories.Length > 0) return categories;
        
        return await GetCategoriesFromUsings(repoFolder);
    }

    private async Task<GameCategory[]> GetCategoriesFromNuget(FilePath projPath)
    {
        if (!_fileSystem.File.Exists(projPath))
        {
            throw new FileNotFoundException($"Could not set included libraries because project file could not be found: {projPath}");
        }

        var queryResult = await _queryNugetListing.Query(projPath, outdated: false);

        var query = queryResult.EvaluateOrThrow();

        return query
            .Select(q => GetRelatedCategory(q.Package))
            .NotNull()
            .ToArray();
    }

    private GameCategory? GetRelatedCategory(string packageName)
    {
        if (!packageName.StartsWith("Mutagen.Bethesda")) return null;
        if (packageName == "Mutagen.Bethesda") return null;
        var split = packageName.Split(".");
        if (split.Length != 3) return null;
        if (Enum.TryParse<GameCategory>(split[2], out var category))
        {
            return category;
        }

        return null;
    }

    private async Task<GameCategory[]> GetCategoriesFromUsings(DirectoryPath repoPath)
    {
        HashSet<GameCategory> ret = new();
        foreach (var csFile in _fileSystem.Directory.EnumerateFilePaths(repoPath, "*.cs", recursive: true))
        {
            var lines = await _fileSystem.File.ReadAllLinesAsync(csFile);
            foreach (var usingLine in lines.Where(l => l.StartsWith("using")))
            {
                if (UsingsToCategories.TryGetValue(usingLine.Trim(), out var gc))
                {
                    ret.Add(gc);
                }
            }
        }

        return ret.ToArray();
    }
}