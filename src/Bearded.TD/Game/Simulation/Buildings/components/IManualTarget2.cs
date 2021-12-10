using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings;

interface IManualTarget2
{
    Position2 Target { get; }
    bool TriggerDown { get; }
}