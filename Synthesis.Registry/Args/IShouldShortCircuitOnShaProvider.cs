﻿namespace Synthesis.Registry.MutagenScraper.Args;

public interface IShouldShortCircuitOnShaProvider
{
    bool ShouldShortCircuit { get; }
    int? RunInvalidation { get; }
}