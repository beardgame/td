using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.GameLoop;

sealed record Wave(
    Id<Wave> Id, WaveScript Script, ImmutableArray<Id<GameObject>> SpawnedObjectIds, Instant DowntimeStart);
