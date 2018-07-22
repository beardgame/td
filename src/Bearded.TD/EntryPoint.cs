using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using Bearded.TD.Game.Components;
using Bearded.Utilities.IO;
using OpenTK;

namespace Bearded.TD
{
    static class EntryPoint
    {
        public static void Main(string[] args)
        {
            var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Directory.SetCurrentDirectory(exeDir ?? throw new InvalidOperationException());

            using (Toolkit.Init(new ToolkitOptions {Backend = PlatformBackend.PreferNative}))
            using (var writer = new StreamWriter(new FileStream(Constants.Paths.LogFile, FileMode.Create,
                FileAccess.ReadWrite, FileShare.ReadWrite)) {AutoFlush = true})
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

                var logger = new Logger {MirrorToConsole = false};

#if DEBUG
                // ReSharper disable once AccessToDisposedClosure
                logger.Logged += entry =>
                {
                    if (entry.Severity == Logger.Severity.Trace) return;
                    writer.WriteLine(entry.Text);
                };
#endif

                logger.Debug?.Log("Creating component factories");
                ComponentFactories.Initialize();

                logger.Info?.Log("");
                logger.Info?.Log("Creating game");
                var game = new TheGame(logger);

                logger.Info?.Log("Running game");
                game.Run(60);

                logger.Info?.Log("Safely exited game");
            }
        }
    }
}
