using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Mutagen.Bethesda;
using Noggog;
using NuGet.Versioning;

namespace Synthesis.Registry.MutagenScraper.Construction;

public class ListedCategoryRetriever
{
    public static readonly SemanticVersion CutoffVersion = new(0, 37, 0);
    
    public GameCategory[] GetListedCategories(string projectContent)
    {
        var releases = new HashSet<GameCategory>();
        foreach (var package in XDocument.Parse(projectContent).Descendants("PackageReference"))
        {
            var include = package.Attribute("Include");
            var vers = package.Attribute("Version");
            if (include == null) continue;
            if (vers == null) continue;
            if (!include.Value.StartsWith("Mutagen.Bethesda")) continue;
            var remainder = include.Value.Substring("Mutagen.Bethesda".Length);
            if (remainder.IsNullOrWhitespace())
            { // Mutagen.Bethesda
                if (SemanticVersion.TryParse(vers.Value, out var semVer)
                    && CutoffVersion > semVer)
                {
                    return new[] { GameCategory.Skyrim };
                }
                else
                {
                    return EnumExt<GameCategory>.Values;
                }
            }
            else if (Enum.TryParse(remainder.AsSpan().Slice(1), out GameCategory cat))
            {
                releases.Add(cat);
            }
        }

        return releases.ToArray();
    }
}