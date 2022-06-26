using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitHubDependents;
using Octokit;

namespace Synthesis.Registry.MutagenScraper.Github;

public class GithubContentDownloader
{
    private readonly ApiUsagePrinter _apiUsagePrinter;
    private readonly GithubClientProvider _githubClientProvider;

    public GithubContentDownloader(ApiUsagePrinter apiUsagePrinter, GithubClientProvider githubClientProvider)
    {
        _apiUsagePrinter = apiUsagePrinter;
        _githubClientProvider = githubClientProvider;
    }

    public async Task<string?> TryGetContent(Dependent dep, string path)
    {
        System.Console.WriteLine($"{dep} retrieving {path}");
        IReadOnlyList<RepositoryContent>? content;
        try
        {
            content = await _githubClientProvider.Client.Repository.Content.GetAllContents(dep.User, dep.Repository, path);
        }
        catch (Exception e)
        {
            System.Console.WriteLine($"{dep} Error getting content for {path}: {e}");
            return null;
        }
        finally
        {
            _apiUsagePrinter.Print();
        }
        if (content.Count == 0)
        {
            System.Console.WriteLine($"{dep} no content found for {path}");
            return null;
        }
        System.Console.WriteLine($"{dep} retrieved {path}");
        return content[0].Content;
    }
}