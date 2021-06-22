using Bearded.Utilities.Geometry;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Simulation
{
    interface ITransformable
    {
        Matrix2 LocalCoordinateTransform { get; }
        Angle LocalOrientationTransform { get; }
    }
}
