using System.IO.Abstractions;
using Autofac;

namespace Synthesis.Registry.MutagenScraper
{
    public class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(new FileSystem())
                .AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(typeof(Program).Assembly)
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}