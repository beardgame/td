using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Core;

readonly record struct WorldCorruptionIncreased(Corruption Value) : IGlobalEvent;

sealed class WorldCorruption(GlobalGameEvents events)
{
    public Corruption Value { get; private set; } = Corruption.Zero;

    public void AddCorruption(Corruption amount)
    {
        Value += amount;

        events.Send(new WorldCorruptionIncreased(Value));
    }
}
