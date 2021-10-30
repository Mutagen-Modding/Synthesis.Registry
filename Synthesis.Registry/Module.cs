using Autofac;

namespace Synthesis.Registry.MutagenScraper
{
    public class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(Program).Assembly)
                .AsImplementedInterfaces();
        }
    }
}