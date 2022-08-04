using Autofac;
using Synthesis.Registry.MutagenScraper.Listings.Specialized;

namespace Synthesis.Registry.MutagenScraper.Modules;

public class RunScraperModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule<MainModule>();
        builder.RegisterType<ManyDependentsToProcessProvider>().AsImplementedInterfaces();
    }
}