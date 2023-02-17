using System.IO.Abstractions;
using Autofac;
using Noggog.Autofac;
using Noggog.Autofac.Modules;
using Noggog.GitRepository;
using Synthesis.Registry.MutagenScraper.Args;
using Synthesis.Registry.MutagenScraper.Listings.Specialized;

namespace Synthesis.Registry.MutagenScraper.Modules;

public class MainModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterInstance(new FileSystem())
            .AsImplementedInterfaces();
        builder.RegisterModule<NoggogModule>();
        builder.RegisterAssemblyTypes(
                typeof(Program).Assembly,
                typeof(IGitRepositoryFactory).Assembly)
            .NotInNamespacesOf(
                typeof(IDependenciesToConsiderIterator),
                typeof(INumToProcessProvider))
            .AsSelf()
            .AsImplementedInterfaces()
            .SingleInstance();
    }
}