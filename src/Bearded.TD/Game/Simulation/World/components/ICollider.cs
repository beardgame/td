using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.World;

interface ICollider
{
    bool TryHit(Ray3 ray, out float rayFactor, out Position3 point, out Difference3 normal);
}
