using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using LibGit2Sharp;
using Noggog;
using Synthesis.Registry.MutagenScraper.Dto;

namespace Synthesis.Registry.MutagenScraper.Github;

public class GetFolderClone
{
    private readonly IFileSystem _fileSystem;
    private readonly DirectoryPath _path = $"./Repositories";
    private readonly HashSet<DirectoryPath> _requested = new();

    public GetFolderClone(
        IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }
    
    public DirectoryPath Get(Listing listing)
    {
        var repoPath = Path.Combine(_path, listing.User, listing.Repository);
        if (_requested.Contains(repoPath)) return repoPath;

        _fileSystem.Directory.DeleteEntireFolder(repoPath);
        var ret = Repository.Clone($"https://github.com/{listing.User}/{listing.Repository}", repoPath);
        
        _requested.Add(repoPath);
        return repoPath;
    }
}