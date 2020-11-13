using System;
using GitHubDependents;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using Synthesis.Bethesda;
using System.Linq;
using System.Net.Http;
using Octokit;
using Noggog;
using Synthesis.Bethesda.DTO;
using System.Text.Json.Serialization;

namespace Synthesis.Registry
{
    class Program
    {
        static JsonSerializerOptions Options = new JsonSerializerOptions()
        {
            WriteIndented = true
        };

        static async Task Main(string[] args)
        {
            var list = await GetGithubDependencies();
            if (list.Failed) return;

            Options.Converters.Add(new JsonStringEnumConverter());

            // Populate metadata about each repository
            var gitHubClient = new GitHubClient(new ProductHeaderValue("SynthesisScraper"));
            var loginToken = args[0];
            gitHubClient.Credentials = new Credentials(loginToken);
            HttpClient client = new HttpClient();
            var repos = (await Task.WhenAll(list.Value
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
                .Select(async dep =>
                {
                    System.Console.WriteLine($"Processing {dep}");
                    var projs = await QueryForProjects(dep, gitHubClient);

                    // Construct listings
                    var patchers = await ConstructListings(dep, gitHubClient, projs);

                    return new RepositoryListing()
                    {
                        AvatarURL = dep.AvatarURL,
                        Repository = dep.Repository!,
                        User = dep.User!,
                        Patchers = patchers.ToArray()
                    };
                })))
                .Where(r =>
                {
                    if (r.Patchers.Length == 0)
                    {
                        System.Console.WriteLine($"{r.Repository} skipped because it had no listed patchers.");
                        return false;
                    }
                    return true;
                })
                .ToArray();
            var limits = gitHubClient.GetLastApiInfo().RateLimit;
            System.Console.WriteLine($"API usage remaining: {(100d * limits.Remaining / (limits.Limit == 0 ? -1 : limits.Limit))}% ({limits.Remaining}/{limits.Limit})");
            System.Console.WriteLine($"Reset at {limits.Reset}");

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

        private static async Task<GetResponse<List<Dependent>>> GetGithubDependencies()
        {
            var list = await GitHubDependents.GitHubDependents.GetDependents("noggog", "synthesis", packageID: "UGFja2FnZS0xMzg1MjY1MjYz", pages: byte.MaxValue);
            if (list.Count == 0)
            {
                System.Console.Error.WriteLine("No repositories retrieved!");
                return GetResponse<List<Dependent>>.Failure;
            }
            System.Console.WriteLine("Retrieved repositories:");
            foreach (var target in list)
            {
                System.Console.WriteLine($"  {target}");
            }
            System.Console.WriteLine();
            System.Console.WriteLine();
            return GetResponse<List<Dependent>>.Succeed(list);
        }

        private static async Task<IEnumerable<string>> QueryForProjects(Dependent dep, GitHubClient gitHubClient)
        {
            var repoColl = new RepositoryCollection();
            repoColl.Add(dep.User, dep.Repository);
            SearchCodeResult? projs = null;
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    projs = await gitHubClient.Search.SearchCode(new SearchCodeRequest()
                    {
                        Extensions = new string[] { "csproj" },
                        Repos = repoColl
                    });
                    if (!projs.IncompleteResults) break;
                    System.Console.WriteLine($"{dep} failed to retrieve patcher listings.  Trying again");
                }
                catch (HttpRequestException)
                {
                    System.Console.WriteLine($"{dep} failed to retrieve patcher listings.  Trying again");
                }
            }

            if (projs?.IncompleteResults ?? true)
            {
                throw new ArgumentException($"Failed to retrieve patcher listings for {dep}");
            }

            return projs.Items.Select(i => i.Path);
        }

        private static async Task<IEnumerable<PatcherListing>> ConstructListings(Dependent dep, GitHubClient gitHubClient, IEnumerable<string> projs)
        {
            return (await Task.WhenAll(projs
                .Select(async proj =>
                {
                    var listing = new PatcherListing()
                    {
                        ProjectPath = proj,
                    };
                    try
                    {
                        var metaPath = Path.Combine(Path.GetDirectoryName(proj)!, Constants.MetaFileName);
                        var content = await gitHubClient.Repository.Content.GetAllContents(dep.User, dep.Repository, metaPath);
                        if (content.Count != 1) return null;
                        var customization = JsonSerializer.Deserialize<PatcherCustomization>(content[0].Content, Options);
                        if (string.IsNullOrWhiteSpace(customization.Nickname))
                        {
                            customization.Nickname = $"{dep.User}/{dep.Repository}";
                        }
                        listing.Customization = customization;
                    }
                    catch (Octokit.NotFoundException)
                    {
                    }
                    return listing;
                })))
                .NotNull()
                .Where(listing => listing.Customization?.Visibility != VisibilityOptions.Exclude)
                .ToArray();
        }
    }
}
