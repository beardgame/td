﻿using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Bearded.TD.Content.Components;
using Bearded.TD.Content.Serialization.Models;
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

        logger.Debug?.Log($".NET Core version: {Environment.Version}");
        logger.Debug?.Log($"Runtime: {RuntimeInformation.FrameworkDescription}");
        logger.Debug?.Log("");

        logger.Debug?.Log("Creating behavior factories");
        ComponentFactories.Initialize();
        FactionBehaviorFactories.Initialize();
        GameRuleFactories.Initialize();
        NodeBehaviorFactories.Initialize();
        TriggerFactories.Initialize();

        logger.Debug?.Log("");

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
