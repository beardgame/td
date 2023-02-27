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
        onHit(@event.Impact.Point);
    }

    private void onHit(Position3 center)
    {
        if (!Owner.TryGetProperty<UntypedDamage>(out var unadjustedDamage))
        {
            DebugAssert.State.IsInvalid();
            return;
        }

        var damage = new UntypedDamage(
            (unadjustedDamage.Amount.NumericValue / Parameters.DamageDivisionFactor).HitPoints());

        var distanceSquared = Parameters.Range.Squared;

        var objects = Owner.Game.CollidableObjectLayer;
        // Returns only tiles with their centre in the circle with the given range.
        // This means it may miss enemies that are strictly speaking in range, but are on a tile that itself is out
        // of range.
        var tiles = Level.TilesWithCenterInCircle(center.XY(), Parameters.Range);

        var damageExecutor = DamageExecutor.FromObject(Owner);

        foreach (var obj in tiles.SelectMany(objects.GetObjectsOnTile))
        {
            var difference = obj.Position - center;

            if (difference.LengthSquared > distanceSquared)
                continue;

            var incident = new Difference3(difference.NumericValue.NormalizedSafe());
            var impact = new Impact(obj.Position, -incident, incident);

            damageExecutor.TryDoDamage(
                obj,
                damage.Typed(Parameters.DamageType ?? DamageType.Kinetic),
                Hit.FromAreaOfEffect(impact));
        }
    }

    public override void Update(TimeSpan elapsedTime) { }
}
