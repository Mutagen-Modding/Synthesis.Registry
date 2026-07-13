using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using GitHubDependents;
using HtmlAgilityPack;
using Noggog;

namespace Synthesis.Registry.MutagenScraper.Listings;

public class GitHubDependentListingsProvider
{
    private const string User = "mutagen-modding";
    private const string Repository = "synthesis";
    private const string PackageId = "UGFja2FnZS0xMzg1MjY1MjYz";

    private const int MaxPages = 500;

    public async Task<GetResponse<List<Dependent>>> Get()
    {
        var list = new List<Dependent>();
        var seenRepos = new HashSet<ListingKeyLite>();
        var seenUrls = new HashSet<string>();

        var web = new HtmlWeb();
        string? url = $"https://github.com/{User}/{Repository}/network/dependents?package_id={PackageId}";

        var page = 0;
        while (url != null && page < MaxPages)
        {
            page++;

            if (!seenUrls.Add(url))
            {
                System.Console.Error.WriteLine($"Dependents pagination revisited a page, stopping early: {url}");
                break;
            }

            HtmlDocument document;
            try
            {
                document = await web.LoadFromWebAsync(url);
            }
            catch (Exception ex)
            {
                System.Console.Error.WriteLine($"Failed to load dependents page {page} ({url}): {ex.Message}");
                break;
            }

            var rows = document.DocumentNode
                .SelectNodes("//div[@data-test-id='dg-repo-pkg-dependent']");
            if (rows != null)
            {
                foreach (var row in rows)
                {
                    var dep = ParseRow(row);
                    if (dep == null) continue;
                    if (seenRepos.Add(new ListingKeyLite(dep.User!, dep.Repository!)))
                    {
                        list.Add(dep);
                    }
                }
            }

            url = FindNextPageUrl(document);
        }

        if (list.Count == 0)
        {
            System.Console.Error.WriteLine("No repositories retrieved!");
            return GetResponse<List<Dependent>>.Failure;
        }

        System.Console.WriteLine($"Retrieved {list.Count} unique repositories across {page} page(s):");
        foreach (var target in list)
        {
            System.Console.WriteLine($"  {target}");
        }
        System.Console.WriteLine();
        System.Console.WriteLine();
        return GetResponse<List<Dependent>>.Succeed(list);
    }

    private static Dependent? ParseRow(HtmlNode row)
    {
        var repoAnchor = row.SelectSingleNode(".//a[@data-hovercard-type='repository']");
        var href = repoAnchor?.GetAttributeValue("href", "");
        if (href.IsNullOrWhitespace()) return null;

        var parts = WebUtility.HtmlDecode(href).TrimStart('/').Split('/');
        if (parts.Length < 2 || parts[0].IsNullOrWhitespace() || parts[1].IsNullOrWhitespace())
        {
            return null;
        }

        var avatar = row.SelectSingleNode(".//img[contains(@class, 'avatar')]")
            ?.GetAttributeValue("src", "");

        return new Dependent
        {
            User = parts[0],
            Repository = parts[1],
            AvatarURL = avatar.IsNullOrWhitespace() ? null : WebUtility.HtmlDecode(avatar),
        };
    }

    private static string? FindNextPageUrl(HtmlDocument document)
    {
        var anchors = document.DocumentNode.SelectNodes(
            "//div[@data-test-selector='pagination']//a");
        if (anchors == null) return null;

        foreach (var anchor in anchors)
        {
            if (!anchor.InnerText.Trim().Equals("Next", StringComparison.OrdinalIgnoreCase)) continue;
            var href = anchor.GetAttributeValue("href", "");
            if (href.IsNullOrWhitespace()) return null;
            // Decode so "...&amp;package_id=..." keeps the package_id filter on the next request.
            return WebUtility.HtmlDecode(href);
        }

        return null;
    }

    private readonly record struct ListingKeyLite(string User, string Repository);
}
