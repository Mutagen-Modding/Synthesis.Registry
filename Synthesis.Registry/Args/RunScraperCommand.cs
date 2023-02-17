using CommandLine;

namespace Synthesis.Registry.MutagenScraper.Args;

public interface IRunScraperCommand
{
    int? RunNumber { get; set; }
    int? NumToProcessPer { get; set; }
}

[Verb("run")]
public class RunScraperCommand : INumToProcessProvider, IRunScraperCommand, IShouldShortCircuitOnShaProvider
{
    [Option('r', "RunNumber", Required = false)]
    public int? RunNumber { get; set; }
    
    [Option('n', "NumToProcessPer", Required = false)]
    public int? NumToProcessPer { get; set; }

    [Option('c', "ShortCircuit", Required = false, HelpText = "Whether to try to short circuit and skip patcher processing")]
    public bool? ShouldShortCircuit { get; set; }

    public int? RunInvalidation { get; }

    [Option('i', "ShortCircuitInvalidationRunNumber", Required = false, HelpText = "Cached values from runs older than this will not be short circuited")]
    public int? ShortCircuitInvalidationRunNumber { get; set; }

    bool IShouldShortCircuitOnShaProvider.ShouldShortCircuit => ShouldShortCircuit ?? true;
    int? IShouldShortCircuitOnShaProvider.RunInvalidation => ShortCircuitInvalidationRunNumber;
}