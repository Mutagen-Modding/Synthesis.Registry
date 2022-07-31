using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Synthesis.Registry.MutagenScraper.Dto;
using Synthesis.Registry.MutagenScraper.Github;

namespace Synthesis.Registry.MutagenScraper
{
    public class QueryForProjects
    {
        private readonly IFileSystem _fileSystem;
        private readonly GetFolderClone _getFolderClone;

        public QueryForProjects(
            IFileSystem fileSystem,
            GetFolderClone getFolderClone)
        {
            _fileSystem = fileSystem;
            _getFolderClone = getFolderClone;
        }
        
        public async Task<IEnumerable<string>> Query(Listing dep)
        {
            var clonePath = _getFolderClone.Get(dep);

            var projs = _fileSystem.Directory.GetFiles(clonePath, "*.csproj", SearchOption.AllDirectories);
            
            var ret = projs
                .OrderBy(Path.GetFileName)
                .ToArray();
            System.Console.WriteLine($"{dep} retrieved project files:{Environment.NewLine}   {string.Join($"{Environment.NewLine}   ", ret)}");
            return ret;
        }
    }
}