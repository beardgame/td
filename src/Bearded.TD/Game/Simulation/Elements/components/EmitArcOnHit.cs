using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements;

[Component("emitArcOnHit")]
sealed class EmitArcOnHit
    : ArcEmitterBase<EmitArcOnHit.IParameters>, IListener<CollideWithLevel>, IListener<CollideWithObject>
{
    public interface IParameters : IArcEmissionParameters, IParametersTemplate<IParameters>
    {
        bool OnHitLevel { get; }
        bool OnHitEnemy { get; }
    }

    private readonly IRanger ranger = new FloodFillRanger();

    public EmitArcOnHit(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        if (Parameters.OnHitEnemy)
        {
            Events.Subscribe<CollideWithObject>(this);
        }

        if (Parameters.OnHitLevel)
        {
            Events.Subscribe<CollideWithLevel>(this);
        }
    }

    public override void OnRemoved()
    {
        if (Parameters.OnHitEnemy)
        {
            Events.Unsubscribe<CollideWithObject>(this);
        }

        if (Parameters.OnHitLevel)
        {
            Events.Unsubscribe<CollideWithLevel>(this);
        }
    }

    public void HandleEvent(CollideWithLevel e)
    {
        onCollide(e.Info);
    }

    public void HandleEvent(CollideWithObject e)
    {
        onCollide(e.Impact);
    }

    private void onCollide(Impact impact)
    {
        if (!Owner.TryGetProperty<UntypedDamage>(out var damage))
        {
            return;
        }

        var range = ranger.GetTilesInRange(
            Owner.Game,
            Owner.Game.PassabilityObserver.GetLayer(Passability.Projectile),
            Level.GetTile(impact.Point.XY()),
            0.U(),
            Parameters.MaxBounceDistance);
        EmitArc(damage, range);
    }

    public override void Activate() { }

    public override void Update(TimeSpan elapsedTime) { }
}
