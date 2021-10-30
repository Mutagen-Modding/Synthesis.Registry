using System.Collections.Generic;
using System.Threading.Tasks;
using GitHubDependents;
using Noggog;

namespace Synthesis.Registry.MutagenScraper.DependentListings
{
    public class SynthesisDependentListingsProvider
    {
        private readonly GitHubDependentListingsProvider _gitHubDependentListingsProvider;
        private readonly ManualDependentListingProvider _manualDependentListingProvider;

        public SynthesisDependentListingsProvider(
            GitHubDependentListingsProvider gitHubDependentListingsProvider,
            ManualDependentListingProvider manualDependentListingProvider)
        {
            _gitHubDependentListingsProvider = gitHubDependentListingsProvider;
            _manualDependentListingProvider = manualDependentListingProvider;
        }
        
        public async Task<GetResponse<List<Dependent>>> Get()
        {
            var resp = await _gitHubDependentListingsProvider.Get();
            if (resp.Failed) return resp;
            var list = resp.Value;

            var manual = await _manualDependentListingProvider.Get();
            if (manual.Succeeded)
            {
                list.AddRange(manual.Value);
            }

            return list;
        }
    }
}