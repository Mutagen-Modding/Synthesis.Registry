using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GitHubDependents;
using Noggog;
using NSubstitute;
using Synthesis.Registry.MutagenScraper.Dto;
using Synthesis.Registry.MutagenScraper.Listings;
using Xunit;

namespace Synthesis.Registry.MutagenScraper.Tests
{
    public class ListingsToProcessProviderTests
    {
        private IReadOnlyList<(Dependent Dependent, Dto.Listing Listing)> Get(int number)
        {
            var ret = new List<(Dependent Dependent, Listing Listing)>();
            for (int i = 0; i < number; i++)
            {
                var dep = new Dependent()
                {
                    Repository = Path.GetRandomFileName(),
                    User = Path.GetRandomFileName(),
                };
                ret.Add((dep,
                    new Listing(
                        AvatarURL: null,
                        Repository: dep.Repository,
                        User: dep.User,
                        Sha: Path.GetRandomFileName())));
            }

            return ret;
        }

        private bool Equality(Listing lhs, Listing rhs)
        {
            return lhs.Repository == rhs.Repository
                   && lhs.User == rhs.User;
        }

        [Theory, BasicAutoData]
        public async Task Typical(ListingsToProcessProvider sut)
        {
            sut.ArgProvider.RunNumber.Returns(1);
            sut.ArgProvider.NumToProcessPer.Returns(3);
            var listings = Get(10);
            sut.DependentsProvider.Get().Returns(
                Task.FromResult((IReadOnlyList<Dependent>)listings.Select(x => x.Dependent).ToArray()));
            var result = sut.Get().ToEnumerable();
            result.Should().Equal(listings.Skip(3).Take(3).Select(x => x.Listing), Equality);
        }

        [Theory, BasicAutoData]
        public async Task Last(ListingsToProcessProvider sut)
        {
            sut.ArgProvider.RunNumber.Returns(3);
            sut.ArgProvider.NumToProcessPer.Returns(3);
            var listings = Get(10);
            sut.DependentsProvider.Get().Returns(
                Task.FromResult((IReadOnlyList<Dependent>)listings.Select(x => x.Dependent).ToArray()));
            var result = sut.Get().ToEnumerable();
            result.Should().Equal(listings.Select(x => x.Listing).Last().AsEnumerable(), Equality);
        }

        [Theory, BasicAutoData]
        public async Task Overflow(ListingsToProcessProvider sut)
        {
            sut.ArgProvider.RunNumber.Returns(4);
            sut.ArgProvider.NumToProcessPer.Returns(3);
            var listings = Get(10);
            sut.DependentsProvider.Get().Returns(
                Task.FromResult((IReadOnlyList<Dependent>)listings.Select(x => x.Dependent).ToArray()));
            var result = sut.Get().ToEnumerable();
            result.Should().Equal(listings.Take(3).Select(x => x.Listing), Equality);
        }
    }
}