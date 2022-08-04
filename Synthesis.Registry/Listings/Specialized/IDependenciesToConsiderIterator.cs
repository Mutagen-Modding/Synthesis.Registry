using System.Collections.Generic;
using GitHubDependents;

namespace Synthesis.Registry.MutagenScraper.Listings.Specialized;

public interface IDependenciesToConsiderIterator
{
    IAsyncEnumerable<Dependent> Get();
}