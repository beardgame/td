using System;
using Bearded.Utilities;

namespace Bearded.TD.Audio;

static class PitchRangeExtensions
{
    public static float ChooseRandomPitch(this PitchRange pitchRange, Random? random = null)
    {
        if (Math.Abs(pitchRange.UpperPitch - pitchRange.LowerPitch) < 1e-4)
        {
            return pitchRange.LowerPitch;
        }

        var t = random?.NextFloat() ?? StaticRandom.Float();
        return pitchRange.LowerPitch + t * (pitchRange.UpperPitch - pitchRange.LowerPitch);
    }
}
