using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Utilities;

static class Matrix2Extensions
{
    public static Difference2 Transform(this Matrix2 transform, Difference2 vector)
    {
        return new Difference2(
            vector.X * transform.M11 + vector.Y * transform.M12,
            vector.X * transform.M21 + vector.Y * transform.M22
        );
    }
}