using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game.Rules;
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
            using (var stream = new FileStream(
                Constants.Paths.LogFile, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (var writer = new StreamWriter(stream))
            {
                try
                {
                    Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

                    var logger = new Logger {MirrorToConsole = true};

                    // ReSharper disable once AccessToDisposedClosure
                    logger.Logged += entry =>
                    {
                        if (entry.Severity == Logger.Severity.Trace) return;
                        writer.WriteLine(entry.Text);
                    };

                    logger.Debug?.Log("Creating behavior factories");
                    ComponentFactories.Initialize();
                    GameRuleFactories.Initialize();

                    logger.Info?.Log("");
                    logger.Info?.Log("Creating game");
                    var game = new TheGame(logger);

                    logger.Info?.Log("Running game");
                    game.Run(60);

                    logger.Info?.Log("Safely exited game");
                }
                catch (Exception e)
                {
                    writer.WriteLine("Bearded.TD ended abruptly");
                    writer.WriteLine(e);
                    throw;
                }
            }
        }
    }
}
