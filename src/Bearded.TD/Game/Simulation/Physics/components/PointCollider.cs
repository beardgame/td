using System;
using System.Collections.Generic;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Physics;

[Component("pointCollider")]
sealed class PointCollider(PointCollider.IParameters parameters)
    : Component<PointCollider.IParameters>(parameters), IPreviewListener<PreviewMove>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(Type = AttributeType.RayRadius)]
        Unit Radius { get; }
    }

    private readonly HashSet<GameObject> objectsHit = [];

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public void PreviewEvent(ref PreviewMove e)
    {
        var (start, step) = e;

        var ray = new Ray3(start, step, Parameters.Radius);

        var rayCast = Owner.Game.Level.CastPiercingRayAgainstObjects(
            ray, Owner.Game.PhysicsLayer, Owner.Game.PassabilityObserver.GetLayer(Passability.Projectile));

        foreach (var (result, t, point, obj, lastStep, normal, tile) in rayCast)
        {
            switch (result)
            {
                case RayCastResultType.HitLevel:
                    Collision.HitLevel(Events, point, step, lastStep, tile);
                    e = new PreviewMove(start, step * t);
                    return;

                case RayCastResultType.HitNothing:
                    var tileHeight = Owner.Game.GeometryLayer[tile].DrawInfo.Height;
                    var aboveTile = point.Z - tileHeight;
                    if (aboveTile < Unit.Zero)
                    {
                        Collision.HitLevel(Events, point, step, null, tile);
                        var z = tileHeight - aboveTile;
                        e = new PreviewMove(start, step.XY().WithZ(z));
                        return;
                    }
                    break;

                case RayCastResultType.HitObject:
                    _ = obj ?? throw new InvalidOperationException();
                    _ = normal ?? throw new InvalidOperationException();
                    if (objectsHit.Add(obj))
                    {
                        Collision.HitObject(Events, point, step, obj, normal.Value, out var solid);
                        if (solid)
                        {
                            e = new PreviewMove(start, step * t);
                            return;
                        }
                    }
                    break;

                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }
}
