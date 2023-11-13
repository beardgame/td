using System;
using System.Collections.Generic;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities.Geometry;
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
            ray, Owner.Game.PhysicsLayer, Owner.Game.PassabilityObserver.GetLayer(Passability.Projectile));

        foreach (var (result, _, point, obj, lastStep, normal, tile) in rayCast)
        {
            switch (result)
            {
                case RayCastResultType.HitNothing:
                    if (point.Z < Owner.Game.GeometryLayer[tile].DrawInfo.Height)
                    {
                        Collision.HitLevel(Events, point, step, null, tile);
                    }
                    break;
                case RayCastResultType.HitLevel:
                    Collision.HitLevel(Events, point, step, lastStep, tile);
                    break;
                case RayCastResultType.HitObject:
                    _ = obj ?? throw new InvalidOperationException();
                    _ = normal ?? throw new InvalidOperationException();
                    if (objectsHit.Add(obj))
                    {
                        Collision.HitObject(Events, point, step, obj, normal.Value, out var solid);
                        if (solid)
                            break;
                    }
                    continue;
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
}
