using LibGit2Sharp;
using Noggog.IO;
using Synthesis.Registry.MutagenScraper.Listings;

namespace Synthesis.Registry.ListingResurrector.Services;

public class Runner
{
    private readonly ExistingListingsProvider _existingListingsProvider;
    
    public Runner(ExistingListingsProvider existingListingsProvider)
    {
        _existingListingsProvider = existingListingsProvider;
    }

    public async Task Run()
    {
        using var dir = TempFolder.Factory();
        LibGit2Sharp.Repository.Clone("https://github.com/Mutagen-Modding/Synthesis.Registry", dir.Dir.Path);
        using var repo = new Repository(dir.Dir.Path);
        foreach (var commit in repo.Commits)
        {
            
        }
    }
}