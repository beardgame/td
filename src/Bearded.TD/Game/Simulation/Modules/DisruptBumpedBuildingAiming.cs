using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Modules;

[Component("disruptBumpedBuildingAiming")]
sealed class DisruptBumpedBuildingAiming(DisruptBumpedBuildingAiming.IParameters parameters)
    : Component<DisruptBumpedBuildingAiming.IParameters>(parameters), IListener<BumpedBuilding>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        TimeSpan Duration { get; }
    }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public void HandleEvent(BumpedBuilding e)
    {
        Owner.Sync(DisruptBuildingAiming.Command, e.Building, Parameters.Duration);
    }
}
