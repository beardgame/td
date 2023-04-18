using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Modules;

[Component("stunBuildingOnStuck")]
sealed class StunBuildingOnStuck : Component<StunBuildingOnStuck.IParameters>, IListener<EnemyGotStuck>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(0.5)]
        TimeSpan Duration { get; }
    }

    public StunBuildingOnStuck(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void HandleEvent(EnemyGotStuck @event)
    {
        var buildings = Owner.Game.BuildingLayer;
        if (!buildings.TryGetMaterializedBuilding(@event.IntendedTarget, out var targetBuilding))
        {
            return;
        }

        Owner.Sync(StunObject.Command, targetBuilding, Parameters.Duration);
    }
}
