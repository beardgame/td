using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Physics;

[Component("softMutualUnitCollider")]
sealed class SoftMutualUnitCollider : Component
{
    private IPhysics physics = null!;
    private IRadius? radiusProvider;
    private Unit radius => radiusProvider?.Radius ?? Unit.Zero;

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IPhysics>(Owner, Events, p => physics = p);
        ComponentDependencies.Depend<IRadius>(Owner, Events, r => radiusProvider = r);
    }

    public override void Activate()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
        var units = Owner.Game.UnitLayer;
        var currentTile = Level.GetTile(Owner.Position);
        var r = radius;

        var acceleration = Acceleration3.Zero;

        foreach (var tile in Tilemap.GetSpiralCenteredAt(currentTile, 1))
        {
            foreach (var unit in units.GetUnitsOnTile(tile))
            {
                if (unit.GetComponents<SoftMutualUnitCollider>().SingleOrDefault() is not { } other)
                    continue;

                var otherRadius = other.radius;
                var difference = unit.Position - Owner.Position;
                var distanceSquared = difference.LengthSquared;

                if (distanceSquared >= (r + otherRadius).Squared)
                    continue;

                var distance = distanceSquared.Sqrt();
                var strength = (r + otherRadius - distance) / 1.S() / 1.S() * 0.5f;
                var direction = -difference.NumericValue.NormalizedSafe();
                acceleration += direction * strength;
            }
        }

        physics.ApplyVelocityImpulse(acceleration * elapsedTime);
    }
}
