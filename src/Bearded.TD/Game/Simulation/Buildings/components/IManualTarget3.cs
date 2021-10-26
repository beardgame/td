using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings
{
    interface IManualTarget3
    {
        Position3 Target { get; }
        bool TriggerDown { get; }
    }
}
