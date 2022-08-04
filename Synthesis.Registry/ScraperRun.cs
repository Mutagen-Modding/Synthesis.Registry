using System;
using System.Linq;
using System.Threading.Tasks;
using Noggog.Tooling.WorkEngine;
using Synthesis.Bethesda.DTO;
using Synthesis.Registry.MutagenScraper.Construction;
using Synthesis.Registry.MutagenScraper.Listings;

namespace Synthesis.Registry.MutagenScraper
{
    public class ScraperRun
    {
        private readonly CleanRemovedPatchers _clean;
        private readonly ExistingListingsProvider _existingListingsProvider;
        private readonly GetRepositoryListingsToUpdate _getRepositoryListingsToUpdate;
        private readonly ExportListings _export;
        private readonly IWorkConsumer _workConsumer;

        public ScraperRun(
            CleanRemovedPatchers clean,
            ExistingListingsProvider existingListingsProvider,
            GetRepositoryListingsToUpdate getRepositoryListingsToUpdate,
            ExportListings export,
            IWorkConsumer workConsumer)
        {
            _clean = clean;
            _existingListingsProvider = existingListingsProvider;
            _getRepositoryListingsToUpdate = getRepositoryListingsToUpdate;
            _export = export;
            _workConsumer = workConsumer;
        }
        
        public async Task Run()
        {
            _workConsumer.Start();
            
            // Clean existing repos that aren't listed anymore
            var existingCleaned = await _clean.Clean(_existingListingsProvider.Listings.Value);
            
            var outbound = existingCleaned.Repositories
                .ToDictionary(x => new ListingKey(x.User, x.Repository), x => x);

            // Get partial section of listings to analyze and fold in this run
            var reposToConsider = await _getRepositoryListingsToUpdate.Get();

            if (reposToConsider.Length == 0)
            {
                Console.WriteLine($"No repos to update.  Exiting");
                return;
            }

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