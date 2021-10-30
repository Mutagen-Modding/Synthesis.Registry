using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GitHubDependents;
using Noggog;
using Octokit;
using Synthesis.Bethesda;
using Synthesis.Bethesda.DTO;
using Synthesis.Registry.MutagenScraper.DependentListings;

namespace Synthesis.Registry.MutagenScraper
{
    public class ScraperRun
    {
        private readonly ApiUsagePrinter _apiUsagePrinter;
        private readonly QueryForProjects _queryForProjects;
        private readonly GithubClientProvider _githubClientProvider;
        private readonly SynthesisDependentListingsProvider _dependentListings;

        static JsonSerializerOptions Options = new()
        {
            WriteIndented = true
        };

        public ScraperRun(
            ApiUsagePrinter apiUsagePrinter,
            QueryForProjects queryForProjects,
            GithubClientProvider githubClientProvider,
            SynthesisDependentListingsProvider dependentListings)
        {
            _apiUsagePrinter = apiUsagePrinter;
            _queryForProjects = queryForProjects;
            _githubClientProvider = githubClientProvider;
            _dependentListings = dependentListings;
        }
        
        public async Task Run()
        {
            var resp = await _dependentListings.Get();
            if (resp.Failed) return;
            var list = resp.Value;

            // Populate metadata about each repository
            var repos = await list
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
                .ToAsyncEnumerable()
                .SelectAwait(async dep =>
                {
                    System.Console.WriteLine($"Processing {dep}");
                    var projs = await _queryForProjects.Query(dep);

                    // Construct listings
                    var patchers = await ConstructListings(dep, _githubClientProvider.Client, projs);
                    System.Console.WriteLine($"Processed {dep} and retrieved {patchers.Length} patchers:{Environment.NewLine}   {string.Join($"{Environment.NewLine}   ", (IEnumerable<PatcherListing>)patchers)}");

                    await Task.Delay(500);
                    return new RepositoryListing()
                    {
                        AvatarURL = dep.AvatarURL,
                        Repository = dep.Repository!,
                        User = dep.User!,
                        Patchers = patchers
                    };
                })
                .Where(r =>
                {
                    if (r.Patchers.Length == 0)
                    {
                        System.Console.WriteLine($"{r.User}/{r.Repository} skipped because it had no listed patchers.");
                        return false;
                    }
                    return true;
                })
                .OrderBy(x => x.Repository)
                .ToArrayAsync();
            _apiUsagePrinter.Print(printReset: true);

            // Write out final listing
            var exportPath = "mutagen-automatic-listing.json";
            File.WriteAllText(exportPath,
                JsonSerializer.Serialize(
                    new MutagenPatchersListing()
                    {
                        Repositories = repos
                    },
                    Options));

            Console.WriteLine($"{exportPath} {(File.Exists(exportPath) ? "exists." : "does not exist!")}");
        }
        
        private async Task<PatcherListing[]> ConstructListings(Dependent dep, GitHubClient gitHubClient, IEnumerable<string> projs)
        {
            return (await projs
                .ToAsyncEnumerable()
                .SelectAwait(async proj =>
                {
                    var listing = new PatcherListing()
                    {
                        ProjectPath = proj,
                    };
                    try
                    {
                        var metaPath = Path.Combine(Path.GetDirectoryName(proj)!, Constants.MetaFileName);
                        System.Console.WriteLine($"{dep} retriving meta path for {proj}");
                        var content = await gitHubClient.Repository.Content.GetAllContents(dep.User, dep.Repository, metaPath);
                        _apiUsagePrinter.Print();
                        if (content.Count != 1)
                        {
                            System.Console.WriteLine($"{dep} no meta path found for {proj}");
                            return null;
                        }
                        System.Console.WriteLine($"{dep} retrived meta path for {proj}");
                        var customization = JsonSerializer.Deserialize<PatcherCustomization>(content[0].Content, Options)!;
                        if (string.IsNullOrWhiteSpace(customization.Nickname))
                        {
                            customization.Nickname = $"{dep.User}/{dep.Repository}";
                        }
                        listing.Customization = customization;

                        // Backwards compatibility
                        try
                        {
                            using var doc = JsonDocument.Parse(content[0].Content);
                            foreach (var elem in doc.RootElement.EnumerateObject())
                            {
                                if (elem.NameEquals("HideByDefault")
                                    && elem.Value.GetBoolean())
                                {
                                    customization.Visibility = VisibilityOptions.IncludeButHide;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Console.WriteLine($"{proj} Error handling backwards compatibility: {ex}");
                        }
                    }
                    catch (Octokit.NotFoundException)
                    {
                    }
                    catch (Octokit.ApiException)
                    {
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"{proj} Error constructing listing: {ex}");
                        return null;
                    }
                    await Task.Delay(500);
                    return listing;
                })
                .ToListAsync())
                .NotNull()
                .Where(listing =>
                {
                    if (listing.Customization?.Visibility == VisibilityOptions.Exclude)
                    {
                        System.Console.WriteLine($"{dep} excluding {listing.ProjectPath}");
                        return false;
                    }
                    return true;
                })
                .ToArray();
        }
    }
}