using System;

namespace Bearded.TD
{
    static class EntryPoint
    {
        public static void Main(string[] args)
        {
            var game = new TheGame();

            game.Run(60);
        }
    }
}
