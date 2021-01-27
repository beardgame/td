using System;
using Bearded.Utilities.IO;
using CommandLine;

namespace Bearded.TD.MasterServer
{
    static class EntryPoint
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineOptions>(args).WithParsed(runServer);
        }

        private static void runServer(CommandLineOptions options)
        {
            var server = new MasterServer(options, new Logger());

            server.Start();

            Console.WriteLine("Master server started. Press 'Q' to quit.");
            while (true)
            {
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Q)
                {
                    break;
                }
            }

            server.Stop();
        }
    }
}
