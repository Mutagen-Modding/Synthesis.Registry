using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;

namespace Synthesis.Registry.MutagenScraper.Tests
{
    public class BasicAutoData : AutoDataAttribute
    {
        public BasicAutoData()
            : base(() =>
            {
                var ret = new Fixture();
                ret.Customize(new AutoNSubstituteCustomization()
                {
                    ConfigureMembers = true
                });
                return ret;
            })
        {
        }
    }
}