using Bearded.TD.Game.GameLoop;
using Bearded.TD.Game.Simulation.Events;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.GameLoop;

readonly record struct WaveStarted(Id<Wave> WaveId, string WaveName, WaveProgress Progress) : IGlobalEvent;
