using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GitHubDependents;
using Noggog;
using Synthesis.Registry.MutagenScraper.Dto;

namespace Synthesis.Registry.MutagenScraper.Listings;

public class BlacklistProvider
{
    public async Task<GetResponse<BlacklistLookup>> Get()
    {
        try
        {
            var manual = JsonSerializer.Deserialize<BlacklistListings>(
                await File.ReadAllTextAsync("blacklist.json"))!;
            return new BlacklistLookup(manual.Blacklist.ToList());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading blacklist: {ex}");
            return GetResponse<BlacklistLookup>.Failure;
        }
    }
}

public class BlacklistLookup
{
    private readonly Dictionary<string, HashSet<string>> _dict = new();
    
    public BlacklistLookup(List<BlacklistEntry> listings)
    {
        _dict = listings
            .ToDictionary(x => x.User, x => x.Repositories.ToHashSet());
    }

    public bool IsBlacklisted(Dependent dependent)
    {
        if (dependent.User == null || dependent.Repository == null) return false;
        if (!_dict.TryGetValue(dependent.User, out var repos)) return false;
        if (repos.Count == 0) return true;
        return repos.Contains(dependent.Repository);
    }
}