using Bearded.TD.Game.GameLoop;
using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.GameLoop;

readonly record struct WaveStarted(Wave Wave, WaveProgress Progress) : IGlobalEvent;
