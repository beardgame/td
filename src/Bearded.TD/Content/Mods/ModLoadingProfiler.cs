using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Bearded.TD.Content.Mods;

sealed class ModLoadingProfiler
{
    public enum LoadingResult
    {
        Success,
        HasWarning,
        HasError,
    }

    public readonly struct BlueprintLoadingProfile
    {
        public string Path { get; }
        public TimeSpan LoadingTime { get; }
        public LoadingResult LoadingResult { get; }

        public BlueprintLoadingProfile(string path, TimeSpan loadingTime, LoadingResult loadingResult)
        {
            Path = path;
            LoadingTime = loadingTime;
            LoadingResult = loadingResult;
        }
    }

    private readonly Stopwatch stopwatch = new();
    private readonly Dictionary<string, TimeSpan> blueprintsLoadingStartTimes = new();

    private readonly object blueprintsLoadingLock = new();
    private readonly List<string> blueprintsLoading = new();
    private readonly object blueprintsLoadedLock = new();
    private readonly List<BlueprintLoadingProfile> blueprintsLoaded = new();

    public ImmutableArray<string> LoadingBlueprints
    {
        get
        {
            lock (blueprintsLoadingLock)
            {
                return ImmutableArray.CreateRange(blueprintsLoading);
            }
        }
    }

    public ImmutableArray<BlueprintLoadingProfile> LoadedBlueprints
    {
        get
        {
            lock (blueprintsLoadedLock)
            {
                return ImmutableArray.CreateRange(blueprintsLoaded);
            }
        }
    }

    public TimeSpan TotalElapsedTime => stopwatch.Elapsed;

    [Conditional("DEBUG")]
    public void StartLoading()
    {
        if (stopwatch.IsRunning)
        {
            throw new InvalidOperationException("Cannot start mod loading more than once.");
        }
        stopwatch.Start();
    }

    [Conditional("DEBUG")]
    public void FinishLoading()
    {
        checkStopwatchRunning();
        lock (blueprintsLoadingLock)
        {
            if (blueprintsLoading.Count > 0)
            {
                throw new InvalidOperationException(
                    "Mod loading finished but some blueprints haven't finished loading.");
            }
        }
        stopwatch.Stop();
    }

    [Conditional("DEBUG")]
    public void StartLoadingBlueprint(string path)
    {
        checkStopwatchRunning();
        lock (blueprintsLoadingLock)
        {
            blueprintsLoading.Add(path);
        }
        blueprintsLoadingStartTimes.Add(path, stopwatch.Elapsed);
    }

    [Conditional("DEBUG")]
    public void FinishLoadingBlueprintSuccessfully(string path)
    {
        finishLoadingBlueprint(path, LoadingResult.Success);
    }

    [Conditional("DEBUG")]
    public void FinishLoadingBlueprintWithWarnings(string path)
    {
        finishLoadingBlueprint(path, LoadingResult.HasWarning);
    }

    [Conditional("DEBUG")]
    public void CleanUpLoadingBlueprint(string path)
    {
        if (blueprintsLoadingStartTimes.ContainsKey(path))
        {
            finishLoadingBlueprint(path, LoadingResult.HasError);
        }
    }

    private void finishLoadingBlueprint(string path, LoadingResult result)
    {
        checkStopwatchRunning();
        var startTime = blueprintsLoadingStartTimes[path];
        var now = stopwatch.Elapsed;
        var elapsed = now - startTime;
        lock (blueprintsLoadingLock)
        {
            blueprintsLoading.Remove(path);
        }
        blueprintsLoadingStartTimes.Remove(path);
        lock (blueprintsLoadedLock)
        {
            blueprintsLoaded.Add(new BlueprintLoadingProfile(path, elapsed, result));
        }
    }

    private void checkStopwatchRunning()
    {
        if (!stopwatch.IsRunning)
        {
            throw new InvalidOperationException("Excepted stopwatch to be running.");
        }
    }
}