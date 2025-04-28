using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Modules;

[Component("stunBumpedBuilding")]
sealed class StunBumpedBuilding(StunBumpedBuilding.IParameters parameters)
    : Component<StunBumpedBuilding.IParameters>(parameters), IListener<BumpedBuilding>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        TimeSpan StunDuration { get; }
    }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public void HandleEvent(BumpedBuilding e)
    {
        Owner.Sync(StunObject.Command, e.Building, Parameters.StunDuration);
    }
}

