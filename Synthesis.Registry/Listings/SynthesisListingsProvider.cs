using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitHubDependents;
using Noggog;

namespace Synthesis.Registry.MutagenScraper.Listings;

public interface ISynthesisDependentsProvider
{
    Task<IReadOnlyList<Dependent>> Get();
}

public class SynthesisDependentsProvider : ISynthesisDependentsProvider
{
    private readonly GitHubDependentListingsProvider _gitHubDependentListingsProvider;
    private readonly ManualListingProvider _manualListingProvider;
    private readonly AsyncLazy<IReadOnlyList<Dependent>> _listings;

    public SynthesisDependentsProvider(
        GitHubDependentListingsProvider gitHubDependentListingsProvider,
        ManualListingProvider manualListingProvider)
    {
        _gitHubDependentListingsProvider = gitHubDependentListingsProvider;
        _manualListingProvider = manualListingProvider;
        _listings = new AsyncLazy<IReadOnlyList<Dependent>>(Fill);
    }
        
    private async Task<IReadOnlyList<Dependent>> Fill()
    {
        var resp = await _gitHubDependentListingsProvider.Get();
        var list = resp.EvaluateOrThrow();

        var manual = await _manualListingProvider.Get();
        if (manual.Succeeded)
        {
            list.AddRange(manual.Value);
        }

        return list
            .Where(dep =>
            {
                if (string.IsNullOrWhiteSpace(dep.Repository))
                {
                    System.Console.WriteLine($"Skipping because there was no repository listed: {dep}");
                    return false;
                }
                if (string.IsNullOrWhiteSpace(dep.User))
                {
                    System.Console.WriteLine($"Skipping because there was no user listed: {dep}");
                    return false;
                }
                return true;
            })
            .ToList();
    }

    public Task<IReadOnlyList<Dependent>> Get() => _listings.Value;
}