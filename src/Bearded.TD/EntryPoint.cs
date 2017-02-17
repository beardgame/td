using System.Globalization;
using System.Threading;
using Bearded.Utilities;
using OpenTK;

namespace Bearded.TD
{
    static class EntryPoint
    {
        public static void Main(string[] args)
        {
            using (Toolkit.Init(new ToolkitOptions() {Backend = PlatformBackend.PreferNative}))
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

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
}
