using System.Collections.Generic;
using System.IO;
using System.Threading;
using Noggog;
using Noggog.GitRepository;
using Synthesis.Registry.MutagenScraper.Dto;

namespace Synthesis.Registry.MutagenScraper.Github;

public interface IGetFolderClone
{
    DirectoryPath Get(InternalRepositoryListing repositoryListing);
}

public class GetFolderClone : IGetFolderClone
{
    private readonly ICheckOrCloneRepo _checkOrCloneRepo;
    private readonly DirectoryPath _path = $"./Repositories";
    private readonly Dictionary<InternalRepositoryListing, DirectoryPath> _cached = new();

    public GetFolderClone(ICheckOrCloneRepo checkOrCloneRepo)
    {
        _checkOrCloneRepo = checkOrCloneRepo;
    }
    
    public DirectoryPath Get(InternalRepositoryListing repositoryListing)
    {
        lock (_cached)
        {
            if (_cached.TryGetValue(repositoryListing, out var val)) return val;
            var remoteDir = $"https://github.com/{repositoryListing.User}/{repositoryListing.Repository}";
            var localDir = Path.Combine(_path, repositoryListing.User, repositoryListing.Repository);
            var result = _checkOrCloneRepo.Check(remoteDir, localDir);
            var ret = result.EvaluateOrThrow();
            val = ret.Local;
            _cached[repositoryListing] = val;
            return val;
        }
    }
}