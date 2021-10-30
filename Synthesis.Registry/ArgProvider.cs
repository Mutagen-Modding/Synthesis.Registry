using System;

namespace Synthesis.Registry.MutagenScraper
{
    public interface IArgProvider
    {
        int RunNumber { get; }
        string LoginToken { get; }
        int NumToProcessPer { get; }
    }

    public class ArgProvider : IArgProvider
    {
        public int RunNumber { get; }
        public string LoginToken { get; }
        public int NumToProcessPer { get; } = 20;

        public ArgProvider()
        {
            var args = Environment.GetCommandLineArgs();
            Console.WriteLine($"Args: {string.Join(' ', args)}");
            LoginToken = args[1];
            RunNumber = int.Parse(args[2]);
            NumToProcessPer = int.Parse(args[3]);
        }
    }
}