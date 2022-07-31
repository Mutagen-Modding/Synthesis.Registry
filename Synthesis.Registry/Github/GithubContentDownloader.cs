using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Synthesis.Registry.MutagenScraper.Dto;

namespace Synthesis.Registry.MutagenScraper.Github;

public class GithubContentDownloader
{
    private readonly IFileSystem _fileSystem;
    private readonly GetFolderClone _getFolderClone;

    public GithubContentDownloader(
        IFileSystem fileSystem,
        GetFolderClone getFolderClone)
    {
        _fileSystem = fileSystem;
        _getFolderClone = getFolderClone;
    }

    public async Task<string?> TryGetContent(Listing dep, string path)
    {
        System.Console.WriteLine($"{dep} retrieving {path}");
        try
        {
            var repoPath = _getFolderClone.Get(dep);
            if (!_fileSystem.File.Exists(path))
            {
                System.Console.WriteLine($"{dep} no content found for {path}");
                return null;
            }
            var ret = _fileSystem.File.ReadAllText(path);
            System.Console.WriteLine($"{dep} retrieved {path}");
            return ret;
        }
        catch (Exception e)
        {
            System.Console.WriteLine($"{dep} Error getting content for {path}: {e}");
            return null;
        }
    }
}