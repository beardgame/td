using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings;

[Component("stunBuildingsOnHit")]
sealed class StunBuildingsOnHit : Component<StunBuildingsOnHit.IParameters>, IListener<CollideWithObject>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        TimeSpan MinDuration { get; }
        TimeSpan MaxDuration { get; }
    }

    public StunBuildingsOnHit(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void Activate()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(CollideWithObject e)
    {
        if (!e.Object.TryGetSingleComponent<IBuildingStateProvider>(out _))
            return;

        var f = StaticRandom.Float();
        var duration = Parameters.MinDuration + f * (Parameters.MaxDuration - Parameters.MinDuration);

        Owner.Sync(StunObject.Command, e.Object, duration);
    }
}

