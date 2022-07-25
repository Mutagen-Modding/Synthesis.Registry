namespace Synthesis.Registry.MutagenScraper.Dto;

public record Listing(
    string? AvatarURL,
    string User,
    string Repository)
{
    public override string ToString()
    {
        return $"{User}/{Repository}";
    }
}