using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using GitHubDependents;
using Octokit;
using Synthesis.Bethesda;
using Synthesis.Bethesda.DTO;

namespace Synthesis.Registry.MutagenScraper;

public class PatcherCustomizationRetriever
{
    private readonly JsonSerializerOptionsProvider _jsonOptions;
    private readonly ApiUsagePrinter _apiUsagePrinter;
    private readonly GithubClientProvider _githubClientProvider;

    public PatcherCustomizationRetriever(
        ApiUsagePrinter apiUsagePrinter,
        GithubClientProvider githubClientProvider, 
        JsonSerializerOptionsProvider jsonOptions)
    {
        _apiUsagePrinter = apiUsagePrinter;
        _githubClientProvider = githubClientProvider;
        _jsonOptions = jsonOptions;
    }

    public async Task<PatcherCustomization?> GetCustomization(Dependent dep, string proj)
    {
        var metaPath = Path.Combine(Path.GetDirectoryName(proj)!, Constants.MetaFileName);
        System.Console.WriteLine($"{dep} retrieving meta path for {proj}");
        IReadOnlyList<RepositoryContent>? content;
        try
        { 
            content = await _githubClientProvider.Client.Repository.Content.GetAllContents(dep.User, dep.Repository, metaPath);
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
        
        return customization;
    }
}