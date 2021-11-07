using System.Linq;
using System.Threading.Tasks;
using Synthesis.Bethesda.DTO;
using Synthesis.Registry.MutagenScraper.Listings;

namespace Synthesis.Registry.MutagenScraper
{
    public class CleanRemovedPatchers
    {
        private readonly ISynthesisListingsProvider _listingsProvider;

        public CleanRemovedPatchers(
            ISynthesisListingsProvider listingsProvider)
        {
            _listingsProvider = listingsProvider;
        }

        public async Task<MutagenPatchersListing> Clean(MutagenPatchersListing existingListings)
        {
            var listed = await _listingsProvider.Get();
            var listedSet = listed
                .Select(x => new ListingKey(x.User, x.Repository))
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