using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements;

[Component("arcEmitter")]
sealed class ArcEmitter : Component<ArcEmitter.IParameters>, IListener<EnemyMarked>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        IComponentOwnerBlueprint Arc { get; }
        TimeSpan LifeTime { get; }
    }

    public ArcEmitter(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void HandleEvent(EnemyMarked @event)
    {
        var arc = ArcFactory.CreateArc(Parameters.Arc, Owner, @event.GameObject, @event.Damage, Parameters.LifeTime);
        Owner.Game.Add(arc);
    }
}
