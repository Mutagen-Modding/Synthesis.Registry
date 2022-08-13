using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Synthesis.Bethesda.DTO;
using Synthesis.Registry.MutagenScraper.Dto;
using Synthesis.Registry.MutagenScraper.Reporting;

namespace Synthesis.Registry.MutagenScraper.Construction
{
    public class ConstructRepositoryListing
    {
        private readonly QueryForProjects _queryForProjects;
        private readonly StateReporter _reporter;
        private readonly ConstructListings _constructListings;

        public ConstructRepositoryListing(
            QueryForProjects queryForProjects,
            StateReporter reporter,
            ConstructListings constructListings)
        {
            _queryForProjects = queryForProjects;
            _reporter = reporter;
            _constructListings = constructListings;
        }
        
        public async Task<RepositoryListing> Construct(InternalRepositoryListing dep, RepositoryListing? existing)
        {
            var projs = await _queryForProjects.Query(dep);
            
            if (projs.Count == 0)
            {
                _reporter.ReportExclusion(dep.Key, $"No patcher projects could be located");
            }

            // Construct listings
            var patchers = await _constructListings.Construct(dep, projs);
            System.Console.WriteLine($"Processed {dep} and retrieved {patchers.Length} patchers:{Environment.NewLine}   {string.Join($"{Environment.NewLine}   ", ((IEnumerable<PatcherListing>)patchers).Select(x => x.ProjectPath))}");

            if (patchers.Length == 0)
            {
                _reporter.ReportExclusion(dep.Key, $"No listings could be constructed");
            }
            
            return new RepositoryListing()
            {
                // For some reason, Avatar was flickering
                AvatarURL = dep.AvatarURL ?? existing?.AvatarURL,
                Repository = dep.Repository!,
                User = dep.User!,
                Patchers = patchers,
                Sha = dep.Sha,
            };
        }
    }
}