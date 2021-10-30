using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GitHubDependents;
using Noggog;
using NSubstitute;
using Synthesis.Registry.MutagenScraper.Listings;
using Xunit;

namespace Synthesis.Registry.MutagenScraper.Tests
{
    public class ListingsToProcessProviderTests
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

        [Theory, BasicAutoData]
        public async Task Typical(ListingsToProcessProvider sut)
        {
            sut.ArgProvider.RunNumber.Returns(1);
            sut.ArgProvider.NumToProcessPer.Returns(3);
            var listings = Get(10);
            sut.ListingsProvider.Get().Returns(Task.FromResult(listings));
            var result = sut.Get().ToEnumerable();
            result.Should().Equal(listings.Skip(3).Take(3));
        }

        [Theory, BasicAutoData]
        public async Task Last(ListingsToProcessProvider sut)
        {
            sut.ArgProvider.RunNumber.Returns(3);
            sut.ArgProvider.NumToProcessPer.Returns(3);
            var listings = Get(10);
            sut.ListingsProvider.Get().Returns(Task.FromResult(listings));
            var result = sut.Get().ToEnumerable();
            result.Should().Equal(listings.Last());
        }

        [Theory, BasicAutoData]
        public async Task Overflow(ListingsToProcessProvider sut)
        {
            sut.ArgProvider.RunNumber.Returns(4);
            sut.ArgProvider.NumToProcessPer.Returns(3);
            var listings = Get(10);
            sut.ListingsProvider.Get().Returns(Task.FromResult(listings));
            var result = sut.Get().ToEnumerable();
            result.Should().Equal(listings.Take(3));
        }
    }
}