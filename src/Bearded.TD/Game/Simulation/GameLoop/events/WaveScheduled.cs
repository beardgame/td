using System;
using Bearded.TD.Game.GameLoop;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameLoop;

delegate void SpawnStartRequirementConsumer(ISpawnStartRequirement requirement);

readonly record struct WaveScheduled(
    Id<Wave> WaveId,
    string WaveName,
    Instant? SpawnStart,
    SpawnStartRequirementConsumer SpawnStartRequirementConsumer,
    Func<bool> CanSummonNow) : IGlobalEvent;
