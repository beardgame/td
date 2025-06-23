using System;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Shared.Events;
using Bearded.Utilities;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings;

[Component("stunBuildingsOnHit")]
sealed class StunBuildingsOnHit : Component<StunBuildingsOnHit.IParameters>, IListener<ObjectHit>
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

    public void HandleEvent(ObjectHit e)
    {
        if (!e.Object.TryGetSingleComponent<IBuildingStateProvider>(out _))
            return;

        var f = Random.Shared.NextFloat();
        var duration = Parameters.MinDuration + f * (Parameters.MaxDuration - Parameters.MinDuration);

        Owner.Sync(StunObject.Command, e.Object, duration);
    }
}

