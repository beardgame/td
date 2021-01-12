using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.GameLoop
{
    readonly struct WaveEnded : IGlobalEvent
    {
        public Id<WaveScript> WaveId { get; }
        public Faction TargetFaction { get; }

        public WaveEnded(Id<WaveScript> waveId, Faction targetFaction)
        {
            WaveId = waveId;
            TargetFaction = targetFaction;
        }
    }
}
