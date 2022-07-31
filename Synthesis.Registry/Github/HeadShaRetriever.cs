using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHubDependents;
using Noggog.Utility;

namespace Synthesis.Registry.MutagenScraper.Github;

public interface IHeadShaRetriever
{
    Task<string?> TryGetSha(Dependent dep);
}

public class HeadShaRetriever : IHeadShaRetriever
{
    private readonly IProcessFactory _processFactory;

    public HeadShaRetriever(
        IProcessFactory processFactory)
    {
        _processFactory = processFactory;
    }

    public async Task<string?> TryGetSha(Dependent dep)
    {
        using var proc = _processFactory.Create(new ProcessStartInfo("git", $"ls-remote https://github.com/{dep.User}/{dep.Repository}"));
        string? headLine = null;
        using var output = proc.Output
            .Where(x => x.Contains("HEAD"))
            .Take(1)
            .Subscribe(x => headLine = x);
        await proc.Run().ConfigureAwait(false);
        return GetShaFromHeadLine(headLine);
    }

    public string? GetShaFromHeadLine(string? headLine)
    {
        if (headLine == null) return null;
        var split = headLine.Split('\t', StringSplitOptions.RemoveEmptyEntries);
        if (split.Length != 2 || split[0].Length != 40) return null;
        return split[0];
    }
}