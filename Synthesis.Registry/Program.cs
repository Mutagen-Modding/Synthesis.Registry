using System;
using GitHubDependents;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace Synthesis.Registry
{
    class Program
    {
        static async Task Main(string[] args)
        {
            List<Dependent> list = await GitHubDependents.GitHubDependents.GetDependents("noggog", "synthesis", packageID: "UGFja2FnZS0xMzg1MjY1MjYz", pages: byte.MaxValue);
            if (list.Count == 0) return;
            foreach (var target in list)
            {
                System.Console.WriteLine($"{target.User}/{target.Repository}");
            }
            File.WriteAllText("patchers.txt", JsonSerializer.Serialize(list, new JsonSerializerOptions()
            {
                WriteIndented = true
            }));
        }
    }
}
