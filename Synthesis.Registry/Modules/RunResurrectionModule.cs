using Autofac;
using Synthesis.Registry.MutagenScraper.Runners;

namespace Synthesis.Registry.MutagenScraper.Modules;

public class RunResurrectionModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule<MainModule>();
        builder.RegisterType<ResurrectRun>().AsImplementedInterfaces();
    }
}