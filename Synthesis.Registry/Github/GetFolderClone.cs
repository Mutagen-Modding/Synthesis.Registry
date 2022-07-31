using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using LibGit2Sharp;
using Noggog;
using Noggog.GitRepository;
using Synthesis.Registry.MutagenScraper.Dto;

namespace Synthesis.Registry.MutagenScraper.Github;

public class GetFolderClone
{
    private readonly ICheckOrCloneRepo _checkOrCloneRepo;
    private readonly DirectoryPath _path = $"./Repositories";

    public GetFolderClone(ICheckOrCloneRepo checkOrCloneRepo)
    {
        _checkOrCloneRepo = checkOrCloneRepo;
    }
    
    public DirectoryPath Get(Listing listing)
    {
        var remoteDir = $"https://github.com/{listing.User}/{listing.Repository}";
        var localDir = Path.Combine(_path, listing.User, listing.Repository);
        var result = _checkOrCloneRepo.Check(remoteDir, localDir, CancellationToken.None);
        var ret = result.EvaluateOrThrow();
        return ret.Local;
    }
}