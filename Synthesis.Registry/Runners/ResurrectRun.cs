using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LibGit2Sharp;
using Noggog.IO;
using Synthesis.Bethesda.DTO;
using Synthesis.Registry.MutagenScraper.Listings;

namespace Synthesis.Registry.MutagenScraper.Runners;

public class ResurrectRun
{
    private readonly ScrapeListingsPathProvider _scrapeListingsPathProvider;
    private readonly PatcherDeadTester _patcherDeadTester;
    private readonly ListingsReader _listingsReader;
    private readonly ExportListings _listingsExport;
    private readonly TargetDirectory _targetDirectory;
    private readonly BlacklistProvider _blacklistLookup;
    
    public ResurrectRun(
        ScrapeListingsPathProvider scrapeListingsPathProvider,
        PatcherDeadTester patcherDeadTester,
        ListingsReader listingsReader,
        ExportListings listingsExport,
        TargetDirectory targetDirectory,
        BlacklistProvider blacklistLookup)
    {
        _scrapeListingsPathProvider = scrapeListingsPathProvider;
        _patcherDeadTester = patcherDeadTester;
        _listingsReader = listingsReader;
        _listingsExport = listingsExport;
        _targetDirectory = targetDirectory;
        _blacklistLookup = blacklistLookup;
    }
    
    public async Task Run()
    {
        using var dir = TempFolder.Factory();
        _targetDirectory.Path = dir.Dir.Path;
        
        LibGit2Sharp.Repository.Clone("https://github.com/Mutagen-Modding/Synthesis.Registry", _targetDirectory.Path);
        using var repo = new Repository(_targetDirectory.Path);

        var existing = _listingsReader.Read(_scrapeListingsPathProvider.Path)
            .Repositories
            .ToDictionary(x => new ListingKey(x.User, x.Repository), x => x);
        
        var testedForDeath = new Dictionary<ListingKey, bool>();
        
        using var client = new HttpClient();

        var blacklist = (await _blacklistLookup.Get()).Value;
        
        foreach (var commit in repo.Commits)
        {
            Commands.Checkout(repo, commit);
            try
            {
                var missingListings = _listingsReader.Read(_scrapeListingsPathProvider.Path)
                    .Repositories
                    .Select(x => new KeyValuePair<ListingKey, RepositoryListing>(
                        new ListingKey(x.User, x.Repository),
                        x))
                    .Where(x => !existing.ContainsKey(x.Key))
                    .ToDictionary(x => x.Key, x => x.Value);

                foreach (var missingListing in missingListings)
                {
                    if (testedForDeath.ContainsKey(missingListing.Key)) continue;
                
                    var isDead = await _patcherDeadTester.IsDead(client, missingListing.Key);
                
                    Console.WriteLine($"{missingListing.Key} was dead? {isDead}");

                    // Don't spam
                    await Task.Delay(250);

                    testedForDeath[missingListing.Key] = isDead;

                    if (!isDead && !blacklist.IsBlacklisted(missingListing.Key))
                    {
                        existing.Add(missingListing.Key, missingListing.Value);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error on commit {commit.Sha}.  Exception: {e}");
            }
        }

        _listingsExport.Write(existing.Values);
        File.Copy(_scrapeListingsPathProvider.Path, Path.Combine(Environment.CurrentDirectory, Path.GetFileName(_scrapeListingsPathProvider.Path)), overwrite: true);
    }
}