﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Synthesis.Bethesda.DTO;
using Synthesis.Registry.MutagenScraper.Dto;

namespace Synthesis.Registry.MutagenScraper.Construction
{
    public class ConstructRepositoryListing
    {
        private readonly QueryForProjects _queryForProjects;
        private readonly ConstructListings _constructListings;

        public ConstructRepositoryListing(
            QueryForProjects queryForProjects,
            ConstructListings constructListings)
        {
            _queryForProjects = queryForProjects;
            _constructListings = constructListings;
        }
        
        public async Task<RepositoryListing> Construct(InternalRepositoryListing dep, RepositoryListing? existing)
        {
            var projs = await _queryForProjects.Query(dep);

            // Construct listings
            var patchers = await _constructListings.Construct(dep, projs);
            System.Console.WriteLine($"Processed {dep} and retrieved {patchers.Length} patchers:{Environment.NewLine}   {string.Join($"{Environment.NewLine}   ", ((IEnumerable<PatcherListing>)patchers).Select(x => x.ProjectPath))}");

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