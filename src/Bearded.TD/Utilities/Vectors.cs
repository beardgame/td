using Bearded.Utilities;
using OpenTK.Mathematics;
using static System.MathF;

namespace Bearded.TD.Utilities;

static class Vectors
{
    public static Vector3 GetRandomUnitVector3()
    {
        var theta = Tau * StaticRandom.Float();
        var phi = Acos(1 - 2 * StaticRandom.Float());
        var sinPhi = Sin(phi);
        return new Vector3(
            sinPhi * Cos(theta),
            sinPhi * Sin(theta),
            Cos(phi)
        );
    }

    public static Vector3d WithZ(this Vector2d xy, double z = 0)
        => new(xy.X, xy.Y, z);
}
