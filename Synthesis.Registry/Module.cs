using System.IO.Abstractions;
using Autofac;
using Noggog.Autofac.Modules;

namespace Synthesis.Registry.MutagenScraper
{
    public class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(new FileSystem())
                .AsImplementedInterfaces();
            builder.RegisterModule<NoggogModule>();
            builder.RegisterAssemblyTypes(typeof(Program).Assembly)
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}