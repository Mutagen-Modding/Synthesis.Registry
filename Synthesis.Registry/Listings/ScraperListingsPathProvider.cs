namespace Synthesis.Registry.MutagenScraper.Listings;

public class ScrapeListingsPathProvider
{
    private readonly TargetDirectory _targetDirectory;
    public string Path => System.IO.Path.Combine(_targetDirectory.Path, "mutagen-automatic-listing.json");

    public ScrapeListingsPathProvider(TargetDirectory targetDirectory)
    {
        _targetDirectory = targetDirectory;
    }
}