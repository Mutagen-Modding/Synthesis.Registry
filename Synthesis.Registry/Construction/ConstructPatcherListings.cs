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

namespace Synthesis.Registry.MutagenScraper.Construction
{
    public class ConstructListings
    {
        private readonly ApiUsagePrinter _apiUsagePrinter;
        private readonly JsonSerializerOptionsProvider _jsonOptions;
        private readonly GithubClientProvider _githubClientProvider;

        public ConstructListings(
            ApiUsagePrinter apiUsagePrinter,
            JsonSerializerOptionsProvider jsonOptions,
            GithubClientProvider githubClientProvider)
        {
            _apiUsagePrinter = apiUsagePrinter;
            _jsonOptions = jsonOptions;
            _githubClientProvider = githubClientProvider;
        }
        
        public async Task<PatcherListing[]> Construct(Dependent dep, IEnumerable<string> projs)
        {
            var gitHubClient = _githubClientProvider.Client;
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
                        System.Console.WriteLine($"{dep} retrieving meta path for {proj}");
                        IReadOnlyList<RepositoryContent>? content;
                        try
                        { 
                            content = await gitHubClient.Repository.Content.GetAllContents(dep.User, dep.Repository, metaPath);
                        }
                        catch (Exception e)
                        {
                            System.Console.WriteLine($"{dep} no meta path found for {proj}");
                            return null;
                        }
                        _apiUsagePrinter.Print();
                        if (content.Count != 1)
                        {
                            System.Console.WriteLine($"{dep} no meta path found for {proj}");
                            return null;
                        }
                        System.Console.WriteLine($"{dep} retrieved meta path for {proj}");
                        var customization = JsonSerializer.Deserialize<PatcherCustomization>(content[0].Content, _jsonOptions.Options)!;
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