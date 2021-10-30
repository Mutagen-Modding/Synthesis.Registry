using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitHubDependents;
using Synthesis.Bethesda.DTO;

namespace Synthesis.Registry.MutagenScraper.Construction
{
    public class ConstructRepositoryListings
    {
        private readonly QueryForProjects _queryForProjects;
        private readonly ConstructListings _constructListings;

        public ConstructRepositoryListings(
            QueryForProjects queryForProjects,
            ConstructListings constructListings)
        {
            _queryForProjects = queryForProjects;
            _constructListings = constructListings;
        }
        
        public async Task<RepositoryListing> Construct(Dependent dep)
        {
            System.Console.WriteLine($"Processing {dep}");
            var projs = await _queryForProjects.Query(dep);

            // Construct listings
            var patchers = await _constructListings.Construct(dep, projs);
            System.Console.WriteLine($"Processed {dep} and retrieved {patchers.Length} patchers:{Environment.NewLine}   {string.Join($"{Environment.NewLine}   ", (IEnumerable<PatcherListing>)patchers)}");

            await Task.Delay(500);
            return new RepositoryListing()
            {
                AvatarURL = dep.AvatarURL,
                Repository = dep.Repository!,
                User = dep.User!,
                Patchers = patchers
            };
        }
    }
}