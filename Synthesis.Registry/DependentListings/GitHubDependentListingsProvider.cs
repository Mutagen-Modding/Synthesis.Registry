using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitHubDependents;
using Noggog;

namespace Synthesis.Registry.MutagenScraper.DependentListings
{
    public class GitHubDependentListingsProvider
    {
        public async Task<GetResponse<List<Dependent>>> Get()
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
    }
}