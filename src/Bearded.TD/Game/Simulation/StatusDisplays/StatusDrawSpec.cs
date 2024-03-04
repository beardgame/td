using System;
using System.Collections.Generic;
using Bearded.TD.Content.Mods;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

static class StatusDrawSpec
{
    public static IStatusDrawSpec StaticIcon(ModAwareSpriteId icon)
    {
        return new StaticIconDrawSpec(icon);
    }

    private sealed record StaticIconDrawSpec(ModAwareSpriteId Icon) : IStatusDrawSpec
    {
        public double? Progress => null;
    }

    public static IStatusDrawSpec StaticIconWithProgress(ModAwareSpriteId icon, Func<double> getProgress)
    {
        return new StaticIconWithProgressDrawSpec(icon, getProgress);
    }

    private sealed class StaticIconWithProgressDrawSpec(ModAwareSpriteId icon, Func<double> getProgress)
        : IStatusDrawSpec
    {
        public ModAwareSpriteId Icon => icon;
        public double? Progress => getProgress();
    }

    public static IStatusDrawSpec BucketedIconWithProgress(
        IReadOnlyList<ModAwareSpriteId> icons, Func<double> getProgress)
    {
        return new BucketedIconWithProgressDrawSpec(icons, getProgress);
    }

    private sealed class BucketedIconWithProgressDrawSpec(
        IReadOnlyList<ModAwareSpriteId> icons, Func<double> getProgress)
        : IStatusDrawSpec
    {
        public ModAwareSpriteId Icon => icons[iconIndex];
        public double? Progress => getProgress();
        private int iconIndex
        {
            get
            {
                if (icons.Count <= 1)
                {
                    return 0;
                }
                var p = getProgress().Clamped(0, 1);
                var bucketSize = 1.0f / (icons.Count - 1);
                return MoreMath.RoundToInt(p / bucketSize);
            }
        }
    }
}
