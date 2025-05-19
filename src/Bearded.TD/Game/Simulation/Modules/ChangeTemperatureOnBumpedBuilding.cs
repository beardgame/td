using Bearded.TD.Game.Simulation.Elements;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Shared.Events;
using static Bearded.TD.Game.Simulation.Modules.ChangeTemperatureOnBumpedBuilding;

namespace Bearded.TD.Game.Simulation.Modules;

[Component("changeTemperatureOfBumpedBuilding")]
sealed class ChangeTemperatureOnBumpedBuilding(IParameters parameters)
    : Component<IParameters>(parameters), IListener<BumpedBuilding>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        TemperatureDifference TemperatureChange { get; }
    }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public void HandleEvent(BumpedBuilding e)
    {
        if (e.Building.TryGetSingleComponent<ITemperatureEventReceiver>(out var temperature))
        {
            temperature.ApplyImmediateTemperatureChange(Parameters.TemperatureChange);
        }
    }
}
