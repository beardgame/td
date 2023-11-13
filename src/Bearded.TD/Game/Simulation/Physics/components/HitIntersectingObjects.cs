using System.Collections.Generic;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Simulation.Physics;

[Component("hitIntersectingObjects")]
sealed class HitIntersectingObjects : Component
{
    private readonly HashSet<GameObject> objectsHit = new();

    protected override void OnAdded() { }

    public override void Activate() { }

    public override void Update(TimeSpan elapsedTime)
    {
        var dir = new Difference3(Vector3.UnitZ);
        var ray = new Ray3(Owner.Position, dir);
        var results = Owner.Game.Level.CastPiercingRayAgainstObjects(ray, Owner.Game.PhysicsLayer, _ => true);

        foreach (var result in results)
        {
            if (result.Type != RayCastResultType.HitObject || result.Object is not { } obj || objectsHit.Contains(obj))
            {
                continue;
            }
            Collision.HitObject(Events, result.Point, dir, obj, -dir, out _);
            objectsHit.Add(obj);
        }
    }
}
