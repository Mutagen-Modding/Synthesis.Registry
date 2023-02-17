using System;
using GitHubDependents;

namespace Synthesis.Registry.MutagenScraper.Dto;

public class ManualListings
{
    public Dependent[] Listings { get; set; } = Array.Empty<Dependent>();
}