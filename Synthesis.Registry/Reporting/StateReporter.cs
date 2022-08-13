using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Noggog;
using Synthesis.Bethesda.DTO;
using Synthesis.Registry.MutagenScraper.Listings;

namespace Synthesis.Registry.MutagenScraper.Reporting;

public class StateReporter
{
    private readonly IFileSystem _fileSystem;
    private readonly JsonSerializerOptionsProvider _jsonOptions;
    private readonly ISynthesisDependentsProvider _dependentsProvider;
    private readonly Dictionary<ListingKey, Listing> _listings = new();

    public record Listing(string? OverallExcludeReason, Dictionary<string, ProjectReportListing> Projects);

    public string JsonPath => "scrape-state.json";
    public string TxtPath => "scrape-state.txt";

    public StateReporter(
        ISynthesisDependentsProvider dependentsProvider, 
        JsonSerializerOptionsProvider jsonOptions, IFileSystem fileSystem)
    {
        _dependentsProvider = dependentsProvider;
        _jsonOptions = jsonOptions;
        _fileSystem = fileSystem;
    }

    public void ReportExclusion(ListingKey key, string exclusionReason)
    {
        if (_listings.ContainsKey(key)) return;
        _listings[key] = new Listing(exclusionReason, new());
    }

    public void ReportExclusion(ListingKey key, string proj, string exclusionReason)
    {
        var listing = _listings.GetOrAdd(key, () => new Listing(null, new()));
        listing.Projects[proj] = new ProjectReportListing(proj, exclusionReason);
    }

    public void ReportProcessed(RepositoryListing listing)
    {
        var key = new ListingKey(listing.User, listing.Repository);
        var reportListing = _listings.GetOrAdd(key, () => new Listing(null, new()));
        foreach (var patcher in listing.Patchers)
        {
            reportListing.Projects.GetOrAdd(patcher.ProjectPath,
                () => new ProjectReportListing(patcher.ProjectPath, null));
        }
    }

    public async Task Export()
    {
        await FillExistingState();
        
        await FillListedDependants();
        
        var report = new Report(
            _listings
                .Select(x => new ReportListing(x.Key.User, x.Key.Repository, x.Value.OverallExcludeReason, x.Value.Projects.Values.ToArray()))
                .OrderBy(x => x.User)
                .ThenBy(x => x.Repository)
                .ToList());
        
        var txt = JsonSerializer.Serialize(
            report,
            _jsonOptions.Options);
            
        await _fileSystem.File.WriteAllTextAsync(JsonPath, txt);

        await using var stream = new StreamWriter(_fileSystem.File.Open(TxtPath, FileMode.Create, FileAccess.Write, FileShare.None));
        
        foreach (var listing in report.Listings)
        {
            var included = listing.ExcludeReason == null && listing.Projects.Any(x => x.ExcludeReason == null);
            await stream.WriteLineAsync($"[{(included ? "X" : " ")}] {listing.User}/{listing.Repository}");
            if (listing.ExcludeReason != null)
            {
                await stream.WriteLineAsync($"    {listing.ExcludeReason}");
            }

            foreach (var projectReportListing in listing.Projects)
            {
                await stream.WriteLineAsync($"    [{(included ? "X" : " ")}] {projectReportListing.Project}");
                if (projectReportListing.ExcludeReason != null)
                {
                    await stream.WriteLineAsync($"        {projectReportListing.ExcludeReason}");
                }
            }
        }
    }

    private async Task FillExistingState()
    {
        if (!_fileSystem.File.Exists(JsonPath)) return;
        
        var report = JsonSerializer.Deserialize<Report>(await _fileSystem.File.ReadAllTextAsync(JsonPath));

        if (report == null) return;
        
        foreach (var listing in report.Listings)
        {
            var key = new ListingKey(listing.User, listing.Repository);
            if (_listings.ContainsKey(key)) continue;
            _listings[key] = new Listing(listing.ExcludeReason, listing.Projects.ToDictionary(x => x.Project));
        }
    }

    private async Task FillListedDependants()
    {
        foreach (var listing in await _dependentsProvider.Get())
        {
            var key = new ListingKey(listing.User!, listing.Repository!);
            if (!_listings.TryGetValue(key, out _))
            {
                _listings[key] = new("Not processed", new());
            }
        }
    }
}