using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Physics;

static class Collision
{
    public static void HitObject(
        ComponentEvents events,
        Position3 point, Difference3 step, GameObject obj, Difference3 normal,
        out bool isSolid)
    {
        var impact = new Impact(point, normal, step.NormalizedSafe());
        events.Send(new TouchObject(obj, impact));

        isSolid = obj.TryGetSingleComponent<ICollider>(out var collider) && collider.IsSolid;

        if (isSolid)
        {
            events.Send(new CollideWithObject(obj, impact));
        }
    }

    public static void HitLevel(
        ComponentEvents events,
        Position3 point, Difference3 step, Direction? withStep, Tile tile)
    {
        TileCollider.HitLevel(events, point, step, withStep, tile);
    }
}
