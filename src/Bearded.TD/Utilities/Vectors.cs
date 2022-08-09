using Bearded.Utilities;
using OpenTK.Mathematics;
using static System.MathF;

namespace Bearded.TD.Utilities;

sealed class Vectors
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
}
