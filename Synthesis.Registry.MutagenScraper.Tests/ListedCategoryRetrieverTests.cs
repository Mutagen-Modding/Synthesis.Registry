using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using FluentAssertions;
using GitHubDependents;
using Mutagen.Bethesda;
using Noggog;
using Synthesis.Bethesda.DTO;
using Synthesis.Registry.MutagenScraper.Construction;
using Xunit;

namespace Synthesis.Registry.MutagenScraper.Tests;

public class ListedCategoryRetrieverTests
{
    private string GetProject(params (string Lib, string Version)[] items)
    {
        return new XElement("Project",
            new XAttribute("Sdk", "Microsoft.NET.Sdk"),
            new XElement("PropertyGroup",
                new XElement("OutputType", "Exe"),
                new XElement("TargetFramework", "net6.0")),
            new XElement("ItemGroup",
                items.Select(item =>
                {
                    return new XElement("PackageReference",
                        new XAttribute("Include", item.Lib),
                        new XAttribute("Version", item.Version));
                }))).ToString();
    }
    
    [Theory, BasicAutoData]
    public void NoIncludesReturnsNoReleases(
        ListedCategoryRetriever sut)
    {
        var projContent = GetProject();
        var targets = sut.GetListedCategories(projContent);
        targets.Should().BeEmpty();
    }
    
    [Theory, BasicAutoData]
    public void TypicalSingleCategory(
        ListedCategoryRetriever sut)
    {
        var projContent = GetProject(("Mutagen.Bethesda.Skyrim", "1.2.3"));
        var targets = sut.GetListedCategories(projContent);
        targets.Should().Equal(GameCategory.Skyrim);
    }
    
    [Theory, BasicAutoData]
    public void TypicalMultipleCategory(
        ListedCategoryRetriever sut)
    {
        var projContent = GetProject(
            ("Mutagen.Bethesda.Skyrim", "1.2.3"),
            ("Mutagen.Bethesda.Fallout4", "1.2.3"));
        var targets = sut.GetListedCategories(projContent);
        targets.Should().Equal(
            GameCategory.Skyrim,
            GameCategory.Fallout4);
    }
    
    [Theory, BasicAutoData]
    public void NewUmbrellaListingIsAll(
        ListedCategoryRetriever sut)
    {
        var projContent = GetProject(
            ("Mutagen.Bethesda.Skyrim", "1.2.3"),
            ("Mutagen.Bethesda", ListedCategoryRetriever.CutoffVersion.ToString()));
        var targets = sut.GetListedCategories(projContent);
        targets.Should().Equal(EnumExt.GetValues<GameCategory>());
    }
    
    [Theory, BasicAutoData]
    public void OldUmbrellaListingIsSkyrimSE(
        ListedCategoryRetriever sut)
    {
        var projContent = GetProject(
            ("Mutagen.Bethesda.Skyrim", "1.2.3"),
            ("Mutagen.Bethesda", "0.0.1"));
        var targets = sut.GetListedCategories(projContent);
        targets.Should().Equal(GameCategory.Skyrim);
    }
}