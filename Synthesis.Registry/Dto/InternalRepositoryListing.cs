using Synthesis.Registry.MutagenScraper.Listings;

namespace Synthesis.Registry.MutagenScraper.Dto;

public record InternalRepositoryListing(
    string? AvatarURL,
    string User,
    string Repository,
    string Sha)
{
    public override string ToString()
    {
        return $"{User}/{Repository}";
    }

    public ListingKey Key => new(User, Repository);
}