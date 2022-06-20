using System.Linq;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Projectiles;

[Component("splashDamageOnHit")]
sealed class ProjectileSplashDamage : Component<ProjectileSplashDamage.IParameters>,
    IListener<HitLevel>, IListener<HitEnemy>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(Type = AttributeType.SplashRange)]
        Unit Range { get; }

        DamageType? DamageType { get; }
    }

    public ProjectileSplashDamage(IParameters parameters) : base(parameters) {}

    protected override void OnAdded()
    {
        Events.Subscribe<HitLevel>(this);
        Events.Subscribe<HitEnemy>(this);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe<HitLevel>(this);
        Events.Unsubscribe<HitEnemy>(this);
    }

    public void HandleEvent(HitLevel @event)
    {
        onHit(@event.Info.Point);
    }

    public void HandleEvent(HitEnemy @event)
    {
        onHit(@event.Info.Point);
    }

    private void onHit(Position3 center)
    {
        if (!Owner.TryGetProperty<UntypedDamage>(out var damage))
        {
            DebugAssert.State.IsInvalid();
            return;
        }

        var distanceSquared = Parameters.Range.Squared;

        var enemies = Owner.Game.UnitLayer;
        // Returns only tiles with their centre in the circle with the given range.
        // This means it may miss enemies that are strictly speaking in range, but are on a tile that itself is out
        // of range.
        var tiles = Level.TilesWithCenterInCircle(center.XY(), Parameters.Range);

        var damageExecutor = DamageExecutor.FromObject(Owner);

        foreach (var enemy in tiles.SelectMany(enemies.GetUnitsOnTile))
        {
            if ((enemy.Position - center).LengthSquared <= distanceSquared)
            {
                damageExecutor.TryDoDamage(enemy, damage.Typed(Parameters.DamageType ?? DamageType.Kinetic));
            }
        }
    }

    public override void Update(TimeSpan elapsedTime) { }
}
