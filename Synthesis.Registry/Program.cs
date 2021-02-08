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
            var repos = await list.Value
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
                    var projs = await QueryForProjects(dep, gitHubClient);

                    // Construct listings
                    var patchers = await ConstructListings(dep, gitHubClient, projs);
                    System.Console.WriteLine($"Processed {dep}");

                    await Task.Delay(500);
                    return new RepositoryListing()
                    {
                        AvatarURL = dep.AvatarURL,
                        Repository = dep.Repository!,
                        User = dep.User!,
                        Patchers = patchers.ToArray()
                    };
                })
                .Where(r =>
                {
                    if (r.Patchers.Length == 0)
                    {
                        System.Console.WriteLine($"{r.Repository} skipped because it had no listed patchers.");
                        return false;
                    }
                    return true;
                })
                .OrderBy(x => x.Repository)
                .ToArrayAsync();
            PrintApiUsage(gitHubClient, printReset: true);

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
            var list = await GitHubDependents.GitHubDependents.GetDependents("mutagen-modding", "synthesis", packageID: "UGFja2FnZS0xMzg1MjY1MjYz", pages: byte.MaxValue).ToListAsync();
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
                    System.Console.WriteLine($"{dep} searching for project files");
                    projs = await gitHubClient.Search.SearchCode(new SearchCodeRequest()
                    {
                        Extensions = new string[] { "csproj" },
                        Repos = repoColl
                    });
                    await PrintAndWaitForReset(gitHubClient);
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
                throw new ArgumentException($"{dep} failed to retrieve patcher listings");
            }

            System.Console.WriteLine($"{dep} retrieved project files");
            return projs.Items
                .OrderBy(i => i.Name)
                .Select(i => i.Path);
        }

        private static async Task<IEnumerable<PatcherListing>> ConstructListings(Dependent dep, GitHubClient gitHubClient, IEnumerable<string> projs)
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
                        System.Console.WriteLine($"{dep} retriving meta path");
                        var content = await gitHubClient.Repository.Content.GetAllContents(dep.User, dep.Repository, metaPath);
                        PrintApiUsage(gitHubClient);
                        if (content.Count != 1) return null;
                        System.Console.WriteLine($"{dep} retrived meta path");
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
                .Where(listing => listing.Customization?.Visibility != VisibilityOptions.Exclude)
                .ToArray();
        }

        private static RateLimit PrintApiUsage(GitHubClient gitHubClient, bool printReset = false)
        {
            var limits = gitHubClient.GetLastApiInfo().RateLimit;
            System.Console.WriteLine($"API usage remaining: {(100d * limits.Remaining / (limits.Limit == 0 ? -1 : limits.Limit))}% ({limits.Remaining}/{limits.Limit})");
            if (printReset)
            {
                System.Console.WriteLine($"Reset at {limits.Reset}");
            }
            return limits;
        }

        private static async Task PrintAndWaitForReset(GitHubClient gitHubClient, bool printReset = false)
        {
            var limits = PrintApiUsage(gitHubClient, printReset);
            if (limits.Remaining == 0)
            {
                var millis = (int)(limits.Reset - DateTime.Now).TotalMilliseconds;
                millis += 2000;
                if (millis > 0)
                {
                    System.Console.WriteLine($"Waiting {millis}ms for API to reset");
                    await Task.Delay(millis);
                }
            }
        }
    }
}
