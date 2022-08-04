using CommandLine;

namespace Synthesis.Registry.MutagenScraper.Args;

[Verb("run-single")]
public class RunSingleScrapeCommand : INumToProcessProvider, IShouldShortCircuitOnShaProvider
{
    [Option('u', "User to analyze", Required = true)]
    public string User { get; set; } = string.Empty;

    [Option('r', "Repository to analyze", Required = true)]
    public string Repository { get; set; } = string.Empty;

    public int? NumToProcessPer => null;

    public bool ShouldShortCircuit => false;
}