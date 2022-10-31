using System.Linq;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Projectiles;

[Component("splashDamageOnHit")]
sealed class SplashDamageOnHit : Component<SplashDamageOnHit.IParameters>,
    IListener<HitLevel>, IListener<HitEnemy>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(Type = AttributeType.SplashRange)]
        Unit Range { get; }

        [Modifiable(3)]
        int DamageDivisionFactor { get; }

        DamageType? DamageType { get; }
    }

    public SplashDamageOnHit(IParameters parameters) : base(parameters) {}

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
        if (!Owner.TryGetProperty<UntypedDamage>(out var unadjustedDamage))
        {
            DebugAssert.State.IsInvalid();
            return;
        }

        var damage = new UntypedDamage(
            StaticRandom.Discretise(
                (float) unadjustedDamage.Amount.NumericValue / Parameters.DamageDivisionFactor).HitPoints());

        var distanceSquared = Parameters.Range.Squared;

        var enemies = Owner.Game.UnitLayer;
        // Returns only tiles with their centre in the circle with the given range.
        // This means it may miss enemies that are strictly speaking in range, but are on a tile that itself is out
        // of range.
        var tiles = Level.TilesWithCenterInCircle(center.XY(), Parameters.Range);

        var damageExecutor = DamageExecutor.FromObject(Owner);

        foreach (var enemy in tiles.SelectMany(enemies.GetUnitsOnTile))
        {
            var difference = enemy.Position - center;

            if (difference.LengthSquared > distanceSquared)
                continue;

            var incident = new Difference3(difference.NumericValue.NormalizedSafe());
            var hitInfo = new HitInfo(enemy.Position, -incident, incident);

            damageExecutor.TryDoDamage(
                enemy,
                damage.Typed(Parameters.DamageType ?? DamageType.Kinetic),
                new HitContext(HitType.AreaOfEffect, hitInfo));
        }
    }

    public override void Update(TimeSpan elapsedTime) { }
}