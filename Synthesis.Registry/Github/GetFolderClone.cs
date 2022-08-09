using System.Collections.Generic;
using System.IO;
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
    private readonly IProvideRepositoryCheckouts _repositoryCheckouts;
    private readonly IResetToLatestMain _resetToLatestMain;
    private readonly DirectoryPath _path = $"./Repositories";
    private readonly Dictionary<InternalRepositoryListing, DirectoryPath> _cached = new();

    public GetFolderClone(
        ICheckOrCloneRepo checkOrCloneRepo,
        IProvideRepositoryCheckouts repositoryCheckouts,
        IResetToLatestMain resetToLatestMain)
    {
        _checkOrCloneRepo = checkOrCloneRepo;
        _repositoryCheckouts = repositoryCheckouts;
        _resetToLatestMain = resetToLatestMain;
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

            using (var repoCheckout = _repositoryCheckouts.Get(ret.Local))
            {
                var repo = repoCheckout.Repository;
                _resetToLatestMain.TryReset(repo);
            }
            val = ret.Local;
            _cached[repositoryListing] = val;
            return val;
        }
    }
}