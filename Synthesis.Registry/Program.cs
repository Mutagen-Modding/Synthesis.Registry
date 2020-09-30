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

namespace Synthesis.Registry
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Retrieve repositories implementing Mutagen.Bethesda.Synthesis
            List<Dependent> list = await GitHubDependents.GitHubDependents.GetDependents("noggog", "synthesis", packageID: "UGFja2FnZS0xMzg1MjY1MjYz", pages: byte.MaxValue);
            if (list.Count == 0)
            {
                System.Console.Error.WriteLine("No repositories retrieved!");
                return;
            }
            System.Console.WriteLine("Retrieved repositories:");
            foreach (var target in list)
            {
                System.Console.WriteLine($"  {target.User}/{target.Repository}");
            }

            // Populate metadata about each repository
            var gitHubClient = new GitHubClient(new ProductHeaderValue("SynthesisScraper"));
            var loginToken = args[0];
            gitHubClient.Credentials = new Credentials(loginToken);
            HttpClient client = new HttpClient();
            var repos = (await Task.WhenAll(list
                .Select(async dep =>
                {
                    var patchers = Array.Empty<PatcherListing>();
                    try
                    {
                        // Get project files within repo
                        var repoColl = new RepositoryCollection();
                        repoColl.Add(dep.User, dep.Repository);
                        var projs = await gitHubClient.Search.SearchCode(new SearchCodeRequest()
                        {
                            Extensions = new string[] { "csproj" },
                            Repos = repoColl
                        });

                        // Construct listings
                        patchers = (await Task.WhenAll(projs.Items
                            .Select(async proj =>
                            {
                                var listing = new PatcherListing()
                                {
                                    ProjectPath = proj.Path,
                                };
                                try
                                {
                                    var metaPath = Path.Combine(Path.GetDirectoryName(proj.Path)!, Constants.MetaFileName);
                                    var content = await gitHubClient.Repository.Content.GetAllContents(dep.User, dep.Repository, metaPath);
                                    if (content.Count != 1) return null;
                                    var customization = JsonSerializer.Deserialize<PatcherCustomization>(content[0].Content);
                                    if (string.IsNullOrWhiteSpace(customization.Nickname))
                                    {
                                        customization.Nickname = $"{dep.User}/{dep.Repository}";
                                    }
                                    listing.Customization = customization;
                                }
                                catch (Octokit.NotFoundException)
                                {
                                }
                                catch (Exception ex)
                                {
                                    System.Console.Error.Write($"Error parsing listing for {dep.User}/{dep.Repository} {proj.Path}: {ex}");
                                    return null;
                                }
                                return listing;
                            })
                            .Select(t => Task.Run(async () => await t))))
                            .NotNull()
                            .ToArray();
                    }
                    catch (Exception ex)
                    {
                        System.Console.Error.WriteLine($"Error processing {dep.User}/{dep.Repository}: {ex}");
                    }

                    return new RepositoryListing()
                    {
                        AvatarURL = dep.AvatarURL,
                        Forks = dep.Forks,
                        Repository = dep.Repository,
                        Stars = dep.Stars,
                        User = dep.User,
                        Patchers = patchers
                    };
                })
                .Select(t => Task.Run(async () => await t))))
                .Where(r => r.Patchers.Length > 0)
                .ToArray();
            System.Console.WriteLine($"API usage: {gitHubClient.GetLastApiInfo().RateLimit})");

            // Write out final listing
            File.WriteAllText("mutagen-listing.json",
                JsonSerializer.Serialize(
                    new MutagenPatchersListing()
                    {
                        Repositories = repos
                    },
                    new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    }));
        }
    }
}
