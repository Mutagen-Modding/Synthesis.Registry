using Octokit;

namespace Synthesis.Registry.MutagenScraper.Github
{
    public class GithubClientProvider
    {
        public GitHubClient Client { get; }

        public GithubClientProvider(
            IArgProvider argProvider)
        {
            var gitHubClient = new GitHubClient(new ProductHeaderValue("SynthesisScraper"));
            gitHubClient.Credentials = new Credentials(argProvider.LoginToken);
            Client = gitHubClient;
        }
    }
}