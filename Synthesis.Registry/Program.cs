using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Synthesis.Registry.MutagenScraper.Args;
using Synthesis.Registry.MutagenScraper.Modules;

namespace Synthesis.Registry.MutagenScraper;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine($"Args: {string.Join(' ', args)}");
        var parser = new Parser();
        await parser.ParseArguments(
                args,
                typeof(RunScraperCommand),
                typeof(RunSingleScrapeCommand),
                typeof(ResurrectCommand))
            .MapResult(
                async (RunScraperCommand cmd) =>
                {
                    await GetRunGivenArgs<RunScraperModule>(cmd).Run();
                    return 0;
                },
                async (RunSingleScrapeCommand cmd) =>
                {
                    await GetRunGivenArgs<RunSingleScraperModule>(cmd).Run();
                    return 0;
                },
                async (ResurrectCommand cmd) =>
                {
                    await GetRunGivenArgs<RunSingleScraperModule>(cmd).Run();
                    return 0;
                },
                async _ => -1);
    }

    static IContainer GetContainer<TModule>(object args)
        where TModule : Module, new()
    {
        var services = new ServiceCollection();
        services.AddLogging();
                        
        var builder = new ContainerBuilder();
        builder.RegisterModule<TModule>();
        builder.RegisterInstance(args).AsSelf().AsImplementedInterfaces();
        builder.Populate(services);

        return builder.Build();
    }

    static ScraperRun GetRunGivenArgs<TModule>(object args)
        where TModule : Module, new()
    {
        var cont = GetContainer<TModule>(args);
        return cont.Resolve<ScraperRun>();
    }
}