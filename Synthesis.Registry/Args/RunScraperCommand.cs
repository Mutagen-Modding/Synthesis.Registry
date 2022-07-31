using CommandLine;

namespace Synthesis.Registry.MutagenScraper;

[Verb("run")]
public class RunScraperCommand
{
    [Option('r', "RunNumber", Required = false)]
    public int RunNumber { get; set; }
    
    [Option('n', "NumToProcessPer", Required = false)]
    public int? NumToProcessPer { get; set; }
}