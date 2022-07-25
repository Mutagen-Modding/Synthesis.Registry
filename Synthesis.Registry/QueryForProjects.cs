using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Octokit;
using Synthesis.Registry.MutagenScraper.Dto;
using Synthesis.Registry.MutagenScraper.Github;
using Synthesis.Registry.MutagenScraper.Listings;

namespace Synthesis.Registry.MutagenScraper
{
    public class QueryForProjects
    {
        private readonly GithubClientProvider _githubClientProvider;
        private readonly ApiUsagePrinter _apiUsagePrinter;

        public QueryForProjects(
            GithubClientProvider githubClientProvider,
            ApiUsagePrinter apiUsagePrinter)
        {
            _githubClientProvider = githubClientProvider;
            _apiUsagePrinter = apiUsagePrinter;
        }
        
        public async Task<IEnumerable<string>> Query(Listing dep)
        {
            var repoColl = new RepositoryCollection();
            repoColl.Add(dep.User, dep.Repository);
            SearchCodeResult? projs = null;
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    System.Console.WriteLine($"{dep} searching for project files");
                    projs = await _githubClientProvider.Client.Search.SearchCode(new SearchCodeRequest()
                    {
                        Extensions = new string[] { "csproj" },
                        Repos = repoColl
                    });
                    await PrintAndWaitForReset();
                    if (projs.IncompleteResults)
                    {
                        System.Console.WriteLine($"{dep} failed to retrieve contained projects.  Trying again");
                    }
                    else if (projs.Items.Count == 0)
                    {
                        System.Console.WriteLine($"{dep} contained projects returned zero.  Trying again");
                    }
                    else
                    {
                        break;
                    }
                    await Task.Delay(5000);
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

            var ret = projs.Items
                .OrderBy(i => i.Name)
                .Select(i => i.Path)
                .ToArray();
            System.Console.WriteLine($"{dep} retrieved project files:{Environment.NewLine}   {string.Join($"{Environment.NewLine}   ", ret)}");
            return ret;
        }

        private async Task PrintAndWaitForReset(bool printReset = false)
        {
            var limits = _apiUsagePrinter.Print(printReset);
            if (limits is { Remaining: 0 })
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