using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Synthesis.Registry.MutagenScraper.Args;
using Synthesis.Registry.MutagenScraper.Modules;

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
                    typeof(RunScraperCommand),
                    typeof(RunSingleScrapeCommand))
                .MapResult(
                    async (RunScraperCommand runScraper) =>
                    {
                        await GetRunGivenArgs<RunScraperModule>(runScraper).Run();
                        return 0;
                    },
                    async (RunSingleScrapeCommand singleScrape) =>
                    {
                        await GetRunGivenArgs<RunSingleScraperModule>(singleScrape).Run();
                        return 0;
                    },
                    async _ => -1);
        }

        static ScraperRun GetRunGivenArgs<TModule>(object args)
            where TModule : Module, new()
        {
            var services = new ServiceCollection();
            services.AddLogging();
                        
            var builder = new ContainerBuilder();
            builder.RegisterModule<TModule>();
            builder.RegisterInstance(args).AsSelf().AsImplementedInterfaces();
            builder.Populate(services);

            var cont = builder.Build();
            return cont.Resolve<ScraperRun>();
        }
    }
}
