using System.Threading.Tasks;
using Synthesis.Bethesda.DTO;
using Synthesis.Registry.MutagenScraper.Dto;

namespace Synthesis.Registry.MutagenScraper.Processing;

public interface IPatcherListingProcessor
{
    Task Process(InternalRepositoryListing repositoryListing, PatcherListing listing);
}
