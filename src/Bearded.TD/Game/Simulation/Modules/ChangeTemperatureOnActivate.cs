using Bearded.TD.Game.Simulation.Elements;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Shared.TechEffects;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Modules;

using static ChangeTemperatureOnActivate;

[Component("changeTemperatureOnActivate")]
sealed class ChangeTemperatureOnActivate(IParameters parameters)
    : Component<IParameters>(parameters)
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(10)]
        TemperatureDifference TemperatureChange { get; }
    }

    protected override void OnAdded()
    {
    }

    public override void Activate()
    {
        if (Owner.TryGetSingleComponent<ITemperatureEventReceiver>(out var temperature))
        {
            temperature.ApplyImmediateTemperatureChange(Parameters.TemperatureChange);
        }
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }
}
