using Octokit;
using Synthesis.Registry.MutagenScraper.Github;

namespace Synthesis.Registry.MutagenScraper
{
    public class ApiUsagePrinter
    {
        private readonly GithubClientProvider _githubClientProvider;

        public ApiUsagePrinter(GithubClientProvider githubClientProvider)
        {
            _githubClientProvider = githubClientProvider;
        }
        
        public RateLimit? Print(bool printReset = false)
        {
            var lastApiInfo = _githubClientProvider.Client.GetLastApiInfo();
            if (lastApiInfo == null) return null;
            var limits = lastApiInfo.RateLimit;
            System.Console.WriteLine($"API usage remaining: {(100d * limits.Remaining / (limits.Limit == 0 ? -1 : limits.Limit))}% ({limits.Remaining}/{limits.Limit})");
            if (printReset)
            {
                System.Console.WriteLine($"Reset at {limits.Reset}");
            }
            return limits;
        }
    }
}