using Autofac;
using Synthesis.Registry.MutagenScraper.Listings.Specialized;

namespace Synthesis.Registry.MutagenScraper.Modules;

public class RunSingleScraperModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule<MainModule>();
        builder.RegisterType<SingleDependentToProcessProvider>().AsImplementedInterfaces();
    }
}