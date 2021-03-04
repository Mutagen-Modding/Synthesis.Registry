using GitHubDependents;
using Synthesis.Bethesda.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synthesis.Registry
{
    public class ManualListings
    {
        public Dependent[] Listings { get; set; } = Array.Empty<Dependent>();
    }
}
