using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using Bearded.Utilities.IO;
using OpenTK;

namespace Bearded.TD
{
    static class EntryPoint
    {
        public static void Main(string[] args)
        {
            var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Directory.SetCurrentDirectory(exeDir);

            using (Toolkit.Init(new ToolkitOptions() {Backend = PlatformBackend.PreferNative}))
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

                var logger = new Logger {MirrorToConsole = false};
                
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
