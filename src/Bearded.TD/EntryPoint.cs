using System.Globalization;
using System.IO;
using System.Threading;
using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.Utilities.IO;
using CommandLine;

namespace Bearded.TD;

static class EntryPoint
{
    public static void Main(string[] args)
    {
        var parser = new Parser(with =>
        {
            with.CaseInsensitiveEnumValues = true;
        });

        parser.ParseArguments<Options>(args)
            .WithParsed(run)
            .WithNotParsed(errors => errors.Output());
    }

    private static void run(Options options)
    {
        using var stream = new FileStream(
            Constants.Paths.LogFile, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        using var writer = new StreamWriter(stream);

#if !DEBUG
        try
        {
#endif
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
        FactionBehaviorFactories.Initialize();
        GameRuleFactories.Initialize();
        NodeBehaviorFactories.Initialize();

        logger.Info?.Log("");
        logger.Info?.Log("Creating game");
        var game = new TheGame(logger, options.Intent);

        logger.Info?.Log("Running game");
        game.Run();

        logger.Info?.Log("Safely exited game");
#if !DEBUG
        }
        catch (System.Exception e)
        {
            writer.WriteLine("Bearded.TD ended abruptly");
            writer.WriteLine(e);
            throw;
        }
#endif
    }
}
