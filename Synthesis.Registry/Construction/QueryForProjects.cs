using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
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

        var included = new List<string>();
        foreach (var proj in projs)
        {
            bool isLibrary;
            try
            {
                isLibrary = await IsLibrary(proj);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{dep} WARNING: skipping project, could not read csproj {proj}: {ex.Message}");
                continue;
            }

            if (isLibrary)
            {
                System.Console.WriteLine($"{dep} skipping library project: {proj}");
                continue;
            }
            included.Add(proj);
        }

        var ret = included
            .Select(x => (FilePath)x)
            .Select(x => x.GetRelativePathTo(clonePath))
            .OrderBy(Path.GetFileName)
            .Select(x => x.Replace('\\', '/'))
            .ToArray();
        System.Console.WriteLine($"{dep} retrieved project files:{Environment.NewLine}   {string.Join($"{Environment.NewLine}   ", ret)}");
        return ret;
    }

    private async Task<bool> IsLibrary(FilePath projPath)
    {
        var content = await _fileSystem.File.ReadAllTextAsync(projPath);
        var outputType = XDocument.Parse(content)
            .Descendants()
            .FirstOrDefault(x => x.Name.LocalName == "OutputType")?.Value;
        return string.Equals(outputType?.Trim(), "Library", StringComparison.OrdinalIgnoreCase);
    }
}