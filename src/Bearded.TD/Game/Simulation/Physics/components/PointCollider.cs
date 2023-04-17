using System;
using System.Collections.Generic;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Physics;

sealed class PointCollider : Component, IPreviewListener<PreviewMove>
{
    private readonly HashSet<GameObject> objectsHit = new();

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public void PreviewEvent(ref PreviewMove e)
    {
        var (start, step) = e;

        var ray = new Ray3(start, step);

        var rayCast = Owner.Game.Level.CastPiercingRayAgainstObjects(
            ray, Owner.Game.PhysicsLayer, Owner.Game.PassabilityManager.GetLayer(Passability.Projectile));

        foreach (var (result, _, point, obj, lastStep, normal, tile) in rayCast)
        {
            switch (result)
            {
                case RayCastResultType.HitNothing:
                    if (point.Z < Owner.Game.GeometryLayer[tile].DrawInfo.Height)
                    {
                        hitLevel(point, step, null, tile);
                    }
                    break;
                case RayCastResultType.HitLevel:
                    hitLevel(point, step, lastStep, tile);
                    break;
                case RayCastResultType.HitObject:
                    _ = obj ?? throw new InvalidOperationException();
                    _ = normal ?? throw new InvalidOperationException();
                    if (objectsHit.Add(obj))
                    {
                        hitObject(point, step, obj, normal.Value);
                    }
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }

            e = new PreviewMove(start, point - start);
            return;
        }
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    private void hitObject(Position3 point, Difference3 step, GameObject enemy, Difference3 normal)
    {
        var info = new Impact(point, normal, step.NormalizedSafe());
        Events.Send(new HitObject(enemy, info));
    }

    private void hitLevel(Position3 point, Difference3 step, Direction? withStep, Tile tile)
    {
        TileCollider.HitLevel(Events, point, step, withStep, tile);
    }
}
