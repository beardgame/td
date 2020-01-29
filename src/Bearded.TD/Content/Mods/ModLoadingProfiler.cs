using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Bearded.TD.Content.Mods
{
    sealed class ModLoadingProfiler
    {
        public enum LoadingResult
        {
            Success,
            HasWarning,
            HasError,
        }

        public struct BlueprintLoadingProfile
        {
            public TimeSpan LoadingTime { get; }
            public LoadingResult LoadingResult { get; }

            public BlueprintLoadingProfile(TimeSpan loadingTime, LoadingResult loadingResult)
            {
                LoadingTime = loadingTime;
                LoadingResult = loadingResult;
            }
        }

        private readonly Stopwatch stopwatch = new Stopwatch();

        private readonly Dictionary<string, TimeSpan> blueprintsLoading = new Dictionary<string, TimeSpan>();

        private readonly Dictionary<string, BlueprintLoadingProfile> blueprintsLoaded =
            new Dictionary<string, BlueprintLoadingProfile>();

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
            if (blueprintsLoading.Count > 0)
            {
                throw new InvalidOperationException(
                    "Mod loading finished but some blueprints haven't finished loading.");
            }
            stopwatch.Stop();
        }

        [Conditional("DEBUG")]
        public void StartLoadingBlueprint(string path)
        {
            checkStopwatchRunning();
            blueprintsLoading.Add(path, stopwatch.Elapsed);
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
            if (blueprintsLoading.ContainsKey(path))
            {
                finishLoadingBlueprint(path, LoadingResult.HasError);
            }
        }

        private void finishLoadingBlueprint(string path, LoadingResult result)
        {
            checkStopwatchRunning();
            var startTime = blueprintsLoading[path];
            var now = stopwatch.Elapsed;
            var elapsed = now - startTime;
            blueprintsLoading.Remove(path);
            blueprintsLoaded.Add(path, new BlueprintLoadingProfile(elapsed, result));
        }

        private void checkStopwatchRunning()
        {
            if (!stopwatch.IsRunning)
            {
                throw new InvalidOperationException("Excepted stopwatch to be running.");
            }
        }
    }
}
