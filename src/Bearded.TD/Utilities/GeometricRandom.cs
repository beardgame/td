using System;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Utilities;

static class GeometricRandom
{
    public static Difference2 UniformRandomPointOnDisk(Random random, Unit radius)
    {
        // https://stackoverflow.com/questions/5837572/generate-a-random-point-within-a-circle-uniformly/50746409#50746409
        var r = radius * MathF.Sqrt(random.NextFloat());
        var direction = RandomDirection(random);
        return Difference2.In(direction, r);
    }

    public static Direction2 RandomDirection(Random random)
    {
        return Direction2.FromRadians(random.NextFloat(MathConstants.TwoPi));
    }
}
