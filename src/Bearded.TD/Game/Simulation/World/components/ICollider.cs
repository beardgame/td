using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.World;

interface ICollider
{
    bool TryHit(Ray ray, out float rayFactor, out Position2 point, out Difference2 normal);
}
