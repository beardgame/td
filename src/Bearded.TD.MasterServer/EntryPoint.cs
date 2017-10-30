using System;
using Bearded.Utilities;

namespace Bearded.TD.MasterServer
{
    static class EntryPoint
    {
        public static void Main(string[] args)
        {
            var server = new MasterServer(new Logger());

            server.Run();

            Console.WriteLine("Master server started. Press 'Q' to quit.");
            while (true)
            {
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Q)
                    break;
            }
        }
    }
}
