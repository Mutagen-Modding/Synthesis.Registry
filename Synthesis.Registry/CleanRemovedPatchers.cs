using System.Linq;
using System.Threading.Tasks;
using Synthesis.Bethesda.DTO;
using Synthesis.Registry.MutagenScraper.Listings;

namespace Synthesis.Registry.MutagenScraper
{
    public class CleanRemovedPatchers
    {
        private readonly ISynthesisDependentsProvider _dependentsProvider;

        public CleanRemovedPatchers(
            ISynthesisDependentsProvider dependentsProvider)
        {
            _dependentsProvider = dependentsProvider;
        }

        public async Task<MutagenPatchersListing> Clean(MutagenPatchersListing existingListings)
        {
            var listed = await _dependentsProvider.Get();
            var listedSet = listed
                .Select(x => new ListingKey(x.User!, x.Repository!))
                .ToHashSet();
            return new MutagenPatchersListing()
            {
                Repositories = existingListings.Repositories
                    .Where(x => listedSet.Contains(new ListingKey(x.User, x.Repository)))
                    .ToArray()
            };
        }
    }
}