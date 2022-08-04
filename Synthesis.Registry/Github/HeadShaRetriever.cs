using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitHubDependents;
using Noggog.Processes.DI;

namespace Synthesis.Registry.MutagenScraper.Github;

public interface IHeadShaRetriever
{
    Task<string?> TryGetSha(Dependent dep, CancellationToken cancellationToken);
}

public class HeadShaRetriever : IHeadShaRetriever
{
    private readonly IProcessRunner _processRunner;

    public HeadShaRetriever(
        IProcessRunner processRunner)
    {
        _processRunner = processRunner;
    }

    public async Task<string?> TryGetSha(Dependent dep, CancellationToken cancellationToken)
    {
        var ret = await _processRunner.RunAndCapture(
            new ProcessStartInfo("git", $"ls-remote https://github.com/{dep.User}/{dep.Repository}"),
            cancellationToken);
        return GetShaFromHeadLine(
            ret.Out
                .Where(x => x.Contains("HEAD"))
                .Take(1)
                .FirstOrDefault());
    }

    public string? GetShaFromHeadLine(string? headLine)
    {
        if (headLine == null) return null;
        var split = headLine.Split('\t', StringSplitOptions.RemoveEmptyEntries);
        if (split.Length != 2 || split[0].Length != 40) return null;
        return split[0];
    }
}