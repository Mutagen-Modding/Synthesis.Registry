using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GitHubDependents;
using Noggog;

namespace Synthesis.Registry.MutagenScraper.DependentListings
{
    public class ManualDependentListingProvider
    {
        public async Task<GetResponse<List<Dependent>>> Get()
        {
            try
            {
                var manual = JsonSerializer.Deserialize<ManualListings>(
                    await File.ReadAllTextAsync(Path.Combine("Synthesis.Registry", "mutagen-manual-dependents.json")))!;
                return manual.Listings.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading manual listing: {ex}");
                return GetResponse<List<Dependent>>.Failure;
            }
        }
    }
}