using Bearded.Utilities.Geometry;
using OpenTK.Mathematics;

namespace Bearded.TD.Utilities;

static class WorldMatrices
{
    public static AffineTransformation Translate(Vector3 offset)
    {
        return new AffineTransformation(Matrix4.CreateTranslation(offset));
    }

    public static LinearTransformation RotateZ(Angle angle)
    {
        return new LinearTransformation(Matrix4.CreateRotationZ(angle.Degrees));
    }

    public static LinearTransformation Scale(float scale)
    {
        return new LinearTransformation(Matrix4.CreateScale(scale));
    }

    public static LinearTransformation Then(this LinearTransformation first, LinearTransformation second)
    {
        return new LinearTransformation(first.Matrix * second.Matrix);
    }

    public static AffineTransformation Then(this LinearTransformation first, AffineTransformation second)
    {
        return new AffineTransformation(first.Matrix * second.Matrix);
    }

    public static AffineTransformation Then(this AffineTransformation first, AffineTransformation second)
    {
        return new AffineTransformation(first.Matrix * second.Matrix);
    }

    public readonly record struct LinearTransformation(Matrix4 Matrix);

    public readonly record struct AffineTransformation(Matrix4 Matrix);
}
