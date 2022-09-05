using System;
using System.Collections.Generic;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Physics;

sealed class PointCollider : Component, IPreviewListener<PreviewMove>
{
    private readonly HashSet<GameObject> enemiesHit = new();

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public void PreviewEvent(ref PreviewMove e)
    {
        var (start, step) = e;

        var ray = new Ray3(start, step);

        var (result, _, point, enemy, lastStep, normal) = Owner.Game.Level.CastRayAgainstEnemies(
            ray, Owner.Game.UnitLayer, Owner.Game.PassabilityManager.GetLayer(Passability.Projectile));

        e = new PreviewMove(start, point - start);

        switch (result)
        {
            case RayCastResultType.HitNothing:
                var tile = Level.GetTile(point.XY());
                if (point.Z < Owner.Game.GeometryLayer[tile].DrawInfo.Height)
                {
                    hitLevel(point, step, null);
                }
                break;
            case RayCastResultType.HitLevel:
                hitLevel(point, step, lastStep);
                break;
            case RayCastResultType.HitEnemy:
                _ = enemy ?? throw new InvalidOperationException();
                _ = normal ?? throw new InvalidOperationException();
                if (enemiesHit.Add(enemy))
                {
                    hitEnemy(point, step, enemy, normal.Value);
                }
                break;
            default:
                throw new IndexOutOfRangeException();
        }
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    private void hitEnemy(Position3 point, Difference3 step, GameObject enemy, Difference3 normal)
    {
        var info = new HitInfo(point, normal, normalized(step));
        Events.Send(new HitEnemy(enemy, info));
    }

    private void hitLevel(Position3 point, Difference3 step, Direction? withStep)
    {
        var normal = new Difference3(withStep?.Vector().WithZ() ?? Vector3.UnitZ);
        var info = new HitInfo(point, normal, normalized(step));
        Events.Send(new HitLevel(info));
    }

    private static Difference3 normalized(Difference3 moveDirection)
        => new(moveDirection.NumericValue.NormalizedSafe());
}
