using System;
using Bearded.TD.Utilities.Console;
using Bearded.Utilities.IO;

namespace Bearded.TD.Meta
{
    static class GlobalCommands
    {
        [Command("quit")]
        private static void quit(Logger logger, CommandParameters p)
        {
            Environment.Exit(0);
        }
    }
}
