using CommandLine;

namespace Synthesis.Registry.MutagenScraper.Args;

public interface IRunScraperCommand
{
    int RunNumber { get; set; }
    int? NumToProcessPer { get; set; }
}

[Verb("run")]
public class RunScraperCommand : INumToProcessProvider, IRunScraperCommand, IShouldShortCircuitOnShaProvider
{
    [Option('r', "RunNumber", Required = false)]
    public int RunNumber { get; set; }
    
    [Option('n', "NumToProcessPer", Required = false)]
    public int? NumToProcessPer { get; set; }

    [Option('c', "ShortCircuit", Required = false)]
    public bool? ShouldShortCircuit { get; set; }

    bool IShouldShortCircuitOnShaProvider.ShouldShortCircuit => ShouldShortCircuit ?? true;
}