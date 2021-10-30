using System.Threading.Tasks;
using Autofac;

namespace Synthesis.Registry.MutagenScraper
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<MainModule>();
            var cont = builder.Build();
            var run = cont.Resolve<ScraperRun>();
            await run.Run();
        }
    }
}
