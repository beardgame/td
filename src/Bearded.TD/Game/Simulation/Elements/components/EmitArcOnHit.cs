using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements;

[Component("emitArcOnHit")]
sealed class EmitArcOnHit
    : ArcEmitterBase<EmitArcOnHit.IParameters>, IListener<CollidedWithLevel>, IListener<ObjectHit>
{
    public interface IParameters : IArcEmissionParameters, IParametersTemplate<IParameters>
    {
        [Modifiable(defaultValue: 1.0f)]
        float FractionOfBaseDamage { get; }

        bool OnHitLevel { get; }
        bool OnHitEnemy { get; }
    }

    private readonly IRanger ranger = new FloodFillRanger();

    public EmitArcOnHit(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        if (Parameters.OnHitEnemy)
        {
            Events.Subscribe<ObjectHit>(this);
        }

        if (Parameters.OnHitLevel)
        {
            Events.Subscribe<CollidedWithLevel>(this);
        }
    }

    protected override void OnRemoved()
    {
        if (Parameters.OnHitEnemy)
        {
            Events.Unsubscribe<ObjectHit>(this);
        }

        if (Parameters.OnHitLevel)
        {
            Events.Unsubscribe<CollidedWithLevel>(this);
        }
    }

    public void HandleEvent(CollidedWithLevel e)
    {
        if (!Owner.TryGetProperty<UntypedDamage>(out var damage))
        {
            return;
        }

        onCollide(e.Info.Point, damage);
    }

    public void HandleEvent(ObjectHit e)
    {
        onCollide(e.Hit.Impact?.Point ?? Owner.Position, e.Hit.DamagePotential);
    }

    private void onCollide(Position3 location, UntypedDamage damage)
    {
        var range = ranger.GetTilesInRange(
            Owner.Game,
            Owner.Game.PassabilityObserver.GetLayer(Passability.Projectile),
            Level.GetTile(location.XY()),
            0.U(),
            Parameters.MaxBounceDistance);
        EmitArc(damage, range);
    }

    public override void Activate() { }

    public override void Update(TimeSpan elapsedTime) { }
}
