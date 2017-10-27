using System;

namespace Bearded.TD.MasterServer
{
    static class EntryPoint
    {
        public static void Main(string[] args)
        {
            var server = new MasterServer();

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
