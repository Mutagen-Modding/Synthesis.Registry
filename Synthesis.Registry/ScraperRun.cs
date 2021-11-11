using System.Linq;
using System.Threading.Tasks;
using Synthesis.Bethesda.DTO;
using Synthesis.Registry.MutagenScraper.Construction;
using Synthesis.Registry.MutagenScraper.Listings;

namespace Synthesis.Registry.MutagenScraper
{
    public class ScraperRun
    {
        private readonly ApiUsagePrinter _apiUsagePrinter;
        private readonly CleanRemovedPatchers _clean;
        private readonly ExistingListingsProvider _existingListingsProvider;
        private readonly GetRepositoryListingsToConsider _getRepositoryListingsToConsider;
        private readonly ExportListings _export;

        public ScraperRun(
            ApiUsagePrinter apiUsagePrinter,
            CleanRemovedPatchers clean,
            ExistingListingsProvider existingListingsProvider,
            GetRepositoryListingsToConsider getRepositoryListingsToConsider,
            ExportListings export)
        {
            _apiUsagePrinter = apiUsagePrinter;
            _clean = clean;
            _existingListingsProvider = existingListingsProvider;
            _getRepositoryListingsToConsider = getRepositoryListingsToConsider;
            _export = export;
        }
        
        public async Task Run()
        {
            // Clean existing repos that aren't listed anymore
            var existingCleaned = await _clean.Clean(_existingListingsProvider.Listings.Value);
            
            var outbound = existingCleaned.Repositories
                .ToDictionary(x => new ListingKey(x.User, x.Repository), x => x);

            // Get partial section of listings to analyze and fold in this run
            var reposToConsider = await _getRepositoryListingsToConsider.Get()
                .ToArrayAsync();
            _apiUsagePrinter.Print(printReset: true);

            foreach (var repositoryListing in reposToConsider)
            {
                var key = new ListingKey(repositoryListing.User, repositoryListing.Repository);
                outbound[key] = repositoryListing;
            }

            // Write out the results
            _export.Write(new MutagenPatchersListing()
            {
                Repositories = outbound.Values
                    .OrderBy(x => x.Repository)
                    .ToArray()
            });
        }
    }
}