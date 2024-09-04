using Bearded.TD.Game.GameLoop;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;

namespace Bearded.TD.Game.Simulation.GameLoop;

readonly record struct WaveEnded(Wave Wave, Faction TargetFaction) : IGlobalEvent;
