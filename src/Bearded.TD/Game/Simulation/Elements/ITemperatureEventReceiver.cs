namespace Bearded.TD.Game.Simulation.Elements;

interface ITemperatureEventReceiver
{
    void ApplyImmediateTemperatureChange(TemperatureDifference difference);
}
