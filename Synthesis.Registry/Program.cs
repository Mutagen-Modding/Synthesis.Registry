using System;
using GitHubDependents;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Synthesis.Registry
{
    class Program
    {
        static async Task Main(string[] args)
        {
            List<Dependent> list = await GitHubDependents.GitHubDependents.GetDependents("noggog", "synthesis", packageID: "UGFja2FnZS0xMzg1MjY1MjYz", pages: byte.MaxValue);

            foreach (var target in list)
            {
                System.Console.WriteLine(target.Repository);
            }
        }
    }
}
