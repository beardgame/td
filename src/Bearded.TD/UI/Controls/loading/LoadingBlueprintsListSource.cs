using System;
using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.UI.Controls;
using Bearded.Utilities;
using OpenTK;

namespace Bearded.TD.UI.Controls
{
    sealed class LoadingBlueprintsListSource : IListItemSource
    {
        private readonly IReadOnlyList<ModLoadingProfiler.BlueprintLoadingProfile> loadedBlueprints;
        private readonly IReadOnlyList<string> loadingBlueprints;

        public LoadingBlueprintsListSource(
            IReadOnlyList<ModLoadingProfiler.BlueprintLoadingProfile> loadedBlueprints,
            IReadOnlyList<string> loadingBlueprints)
        {
            this.loadedBlueprints = loadedBlueprints;
            this.loadingBlueprints = loadingBlueprints;
            ItemCount = loadedBlueprints.Count + loadingBlueprints.Count;
        }

        public double HeightOfItemAt(int index) => 24;

        public Control CreateItemControlFor(int index)
        {
            return index < loadedBlueprints.Count
                ? LoadingBlueprintsListRow.ForLoaded(loadedBlueprints[index])
                : LoadingBlueprintsListRow.ForCurrentlyLoading(loadingBlueprints[index - loadedBlueprints.Count]);
        }

        public void DestroyItemControlAt(int index, Control control)
        {
        }

        public int ItemCount { get; }
    }

    sealed class LoadingBlueprintsListRow : CompositeControl
    {
        private LoadingBlueprintsListRow(string path, Color color, Maybe<TimeSpan> loadingTime)
        {
            Add(new Label(path)
            {
                Color = color, FontSize = 18, TextAnchor = new Vector2d(0, .5)
            }.Anchor(a => a.Right(margin: 100)));
            loadingTime.Select(time =>
                    new Label($"{time:s\\.fff}s")
                    {
                        Color = color, FontSize = 18, TextAnchor = new Vector2d(1, .5)
                    }.Anchor(a => a.Right(width: 100)))
                .Match(Add);
        }

        public static LoadingBlueprintsListRow ForCurrentlyLoading(string path) =>
            new LoadingBlueprintsListRow(path, Color.LightBlue, Maybe.Nothing);

        public static LoadingBlueprintsListRow ForLoaded(ModLoadingProfiler.BlueprintLoadingProfile profile) =>
            new LoadingBlueprintsListRow(profile.Path, color(profile), Maybe.Just(profile.LoadingTime));

        private static Color color(ModLoadingProfiler.BlueprintLoadingProfile profile)
        {
            return profile.LoadingResult switch
            {
                ModLoadingProfiler.LoadingResult.Success => Color.White,
                ModLoadingProfiler.LoadingResult.HasWarning => Color.Orange,
                ModLoadingProfiler.LoadingResult.HasError => Color.Red,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
