using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Synthesis.Registry.MutagenScraper.Listings;

namespace Synthesis.Registry.MutagenScraper;

public class PatcherDeadTester
{
    public async Task<bool> IsDead(HttpClient client, ListingKey listing)
    {
        var url = $"https://github.com/{listing.User}/{listing.Repository}";
        
        HttpResponseMessage response = await client.SendAsync(
            new HttpRequestMessage(HttpMethod.Head, url)
        );

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return true;
        }

        return false;
    }
}