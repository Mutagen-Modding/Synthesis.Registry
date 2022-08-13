namespace Synthesis.Registry.MutagenScraper.Listings;

public record ListingKey(string User, string Repository)
{
    public override string ToString()
    {
        return $"{User}/{Repository}";
    }
}