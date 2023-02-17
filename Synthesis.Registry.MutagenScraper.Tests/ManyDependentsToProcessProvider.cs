using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GitHubDependents;
using Noggog;
using NSubstitute;
using Xunit;

namespace Synthesis.Registry.MutagenScraper.Tests;

public class ManyDependentsToProcessProvider
{
    private IReadOnlyList<Dependent> Get(int number)
    {
        var ret = new List<Dependent>();
        for (int i = 0; i < number; i++)
        {
            ret.Add(new Dependent()
            {
                Repository = Path.GetRandomFileName(),
                User = Path.GetRandomFileName(),
            });
        }

        return ret;
    }

    private bool Equality(Dependent lhs, Dependent rhs)
    {
        return lhs.Repository == rhs.Repository
               && lhs.User == rhs.User;
    }

    [Theory, BasicAutoData]
    public async Task Typical(Listings.Specialized.ManyDependentsToProcessProvider sut)
    {
        sut.ArgProvider.RunNumber.Returns(1);
        sut.ArgProvider.NumToProcessPer.Returns(3);
        var listings = Get(10);
        sut.DependentsProvider.Get().Returns(
            Task.FromResult((IReadOnlyList<Dependent>)listings.ToArray()));
        var result = sut.Get().ToEnumerable().Take(3);
        result.Should().Equal(listings.Skip(3).Take(3), Equality);
    }

    [Theory, BasicAutoData]
    public async Task LastCrossover(Listings.Specialized.ManyDependentsToProcessProvider sut)
    {
        sut.ArgProvider.RunNumber.Returns(3);
        sut.ArgProvider.NumToProcessPer.Returns(3);
        var listings = Get(10);
        sut.DependentsProvider.Get().Returns(
            Task.FromResult((IReadOnlyList<Dependent>)listings.ToArray()));
        var result = sut.Get().ToEnumerable().Take(3);
        result.Should().Equal(new []{ listings[9], listings[0], listings[1] }, Equality);
    }

    [Theory, BasicAutoData]
    public async Task Overflow(Listings.Specialized.ManyDependentsToProcessProvider sut)
    {
        sut.ArgProvider.RunNumber.Returns(4);
        sut.ArgProvider.NumToProcessPer.Returns(3);
        var listings = Get(10);
        sut.DependentsProvider.Get().Returns(
            Task.FromResult((IReadOnlyList<Dependent>)listings.ToArray()));
        var result = sut.Get().ToEnumerable().Take(3);
        result.Should().Equal(listings.Take(3), Equality);
    }
}