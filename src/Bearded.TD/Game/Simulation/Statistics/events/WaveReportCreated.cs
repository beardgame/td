using Bearded.TD.Game.GameLoop;
using Bearded.TD.Game.Simulation.Events;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Statistics;

readonly record struct WaveReportCreated(Id<Wave> WaveId, WaveReport Report) : IGlobalEvent;
