using System.Collections.Generic;

namespace Synthesis.Registry.MutagenScraper.Reporting;

public record Report(List<ReportListing> Listings);

public record ReportListing(string User, string Repository, string? ExcludeReason, ProjectReportListing[] Projects);

public record ProjectReportListing(string Project, string? ExcludeReason);