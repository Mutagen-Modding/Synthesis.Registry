using System;
using GitHubDependents;

namespace Synthesis.Registry.MutagenScraper.Dto;

public record BlacklistEntry
{
    public string User { get; set; } = string.Empty;
    public string[] Repositories { get; set; } = [];
}

public class BlacklistListings
{
    public BlacklistEntry[] Blacklist { get; set; } = [];
}