using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Noggog;
using Synthesis.Registry.MutagenScraper.Dto;
using Synthesis.Registry.MutagenScraper.Github;

namespace Synthesis.Registry.MutagenScraper.Construction;

public class QueryForProjects
{
    private readonly IFileSystem _fileSystem;
    private readonly GetFolderClone _getFolderClone;

    public QueryForProjects(
        IFileSystem fileSystem,
        GetFolderClone getFolderClone)
    {
        _fileSystem = fileSystem;
        _getFolderClone = getFolderClone;
    }
        
    public async Task<IReadOnlyList<string>> Query(InternalRepositoryListing dep)
    {
        var clonePath = _getFolderClone.Get(dep);

        var projs = _fileSystem.Directory.GetFiles(clonePath, "*.csproj", SearchOption.AllDirectories);
            
        var ret = projs
            .Select(x => (FilePath)x)
            .Select(x => x.GetRelativePathTo(clonePath))
            .OrderBy(Path.GetFileName)
            .Select(x => x.Replace('\\', '/'))
            .ToArray();
        System.Console.WriteLine($"{dep} retrieved project files:{Environment.NewLine}   {string.Join($"{Environment.NewLine}   ", ret)}");
        return ret;
    }
}