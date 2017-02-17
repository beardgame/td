using Bearded.Utilities;

namespace Bearded.TD
{
    static class EntryPoint
    {
        public static void Main(string[] args)
        {
            var logger = new Logger();

            logger.Info.Log("");
            logger.Info.Log("Creating game");
            var game = new TheGame(logger);

            logger.Info.Log("Running game");
            game.Run(60);

            logger.Info.Log("Safely exited game");
        }
    }
}
