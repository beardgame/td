using Bearded.Utilities.Geometry;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Simulation;

interface ITransformable
{
    Matrix2 LocalCoordinateTransform { get; }
    Angle LocalOrientationTransform { get; }
}

sealed class Transformable : ITransformable
{
    public static ITransformable Identity { get; } = new Transformable();

    public Matrix2 LocalCoordinateTransform { get; } = Angle.Zero.Transformation;
    public Angle LocalOrientationTransform { get; } = Angle.Zero;

    private Transformable() { }
}