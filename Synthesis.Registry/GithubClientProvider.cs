using System;
using Octokit;

namespace Synthesis.Registry.MutagenScraper
{
    public class GithubClientProvider
    {
        public GitHubClient Client { get; }

        public GithubClientProvider()
        {
            var gitHubClient = new GitHubClient(new ProductHeaderValue("SynthesisScraper"));
            var loginToken = Environment.GetCommandLineArgs()[0];
            gitHubClient.Credentials = new Credentials(loginToken);
            Client = gitHubClient;
        }
    }
}