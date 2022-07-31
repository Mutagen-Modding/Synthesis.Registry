using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace Synthesis.Registry.MutagenScraper
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine($"Args: {string.Join(' ', args)}");
            var parser = new Parser();
            await parser.ParseArguments(
                    args,
                    typeof(RunScraperCommand))
                .MapResult(
                    async (RunScraperCommand runScraper) =>
                    {
                        var services = new ServiceCollection();
                        services.AddLogging();
                        
                        var builder = new ContainerBuilder();
                        builder.RegisterModule<MainModule>();
                        builder.RegisterInstance(runScraper);
                        builder.Populate(services);
                        
                        var cont = builder.Build();
                        var run = cont.Resolve<ScraperRun>();
                        await run.Run();
                        return 0;
                    },
                    async _ => -1);
        }
    }
}
