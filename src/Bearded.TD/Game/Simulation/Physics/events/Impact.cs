using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Simulation.Physics;

readonly record struct Impact(Position3 Point, Difference3 SurfaceNormal, Difference3 IncidentDirection)
{
    public Difference3 GetReflection()
    {
        var dotProduct = Vector3.Dot(SurfaceNormal.NumericValue, IncidentDirection.NumericValue);
        return IncidentDirection - 2 * dotProduct * SurfaceNormal;
    }
}
