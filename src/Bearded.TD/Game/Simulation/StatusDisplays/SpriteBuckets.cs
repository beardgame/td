using System.Collections.Generic;
using Bearded.TD.Content.Mods;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

sealed class SpriteBuckets(IReadOnlyList<ModAwareSpriteId> icons)
{
    public ModAwareSpriteId ResolveIcon(double progress) => icons[resolveIndex(progress)];

    private int resolveIndex(double progress)
    {

        if (icons.Count <= 1)
        {
            return 0;
        }

        var p = progress.Clamped(0, 1);
        var bucketSize = 1.0f / (icons.Count - 1);
        return MoreMath.RoundToInt(p / bucketSize);
    }
}
