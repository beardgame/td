using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using JetBrains.Annotations;

namespace Bearded.TD.Game.GameLoop;

sealed class WaveScriptSerializer
{
    private Id<WaveScript> id;
    private string displayName = "";
    private Id<Faction> targetFaction;
    private double spawnStart;
    private double spawnDuration;
    private int resourcesAwardedBySpawnPhase;
    private Id<SpawnLocation>[] spawnLocations = {};
    private int unitsPerSpawnLocation;
    private ModAwareId unitBlueprint;
    private Id<EnemyUnit>[] spawnedUnitIds = {};

    [UsedImplicitly]
    public WaveScriptSerializer() {}

    public WaveScriptSerializer(WaveScript waveScript)
    {
        id = waveScript.Id;
        displayName = waveScript.DisplayName;
        targetFaction = waveScript.TargetFaction.Id;
        spawnStart = waveScript.SpawnStart.NumericValue;
        spawnDuration = waveScript.SpawnDuration.NumericValue;
        resourcesAwardedBySpawnPhase = waveScript.ResourcesAwardedBySpawnPhase.NumericValue;
        spawnLocations = waveScript.SpawnLocations.Select(loc => loc.Id).ToArray();
        unitsPerSpawnLocation = waveScript.UnitsPerSpawnLocation;
        unitBlueprint = waveScript.UnitBlueprint.Id;
        spawnedUnitIds = waveScript.SpawnedUnitIds.ToArray();
    }

    public WaveScript ToWaveScript(GameInstance game)
    {
        return new(
            id,
            displayName,
            game.State.Factions.Resolve(targetFaction),
            new Instant(spawnStart),
            new TimeSpan(spawnDuration),
            new ResourceAmount(resourcesAwardedBySpawnPhase),
            spawnLocations.Select(loc => game.State.Find(loc)).ToImmutableArray(),
            unitsPerSpawnLocation,
            game.Blueprints.Units[unitBlueprint],
            spawnedUnitIds.ToImmutableArray());
    }

    public void Serialize(INetBufferStream stream)
    {
        stream.Serialize(ref id);
        stream.Serialize(ref displayName);
        stream.Serialize(ref targetFaction);
        stream.Serialize(ref spawnStart);
        stream.Serialize(ref spawnDuration);
        stream.Serialize(ref resourcesAwardedBySpawnPhase);
        stream.SerializeArrayCount(ref spawnLocations);
        for (var i = 0; i < spawnLocations.Length; i++)
        {
            stream.Serialize(ref spawnLocations[i]);
        }
        stream.Serialize(ref unitsPerSpawnLocation);
        stream.Serialize(ref unitBlueprint);
        stream.SerializeArrayCount(ref spawnedUnitIds);
        for (var i = 0; i < spawnedUnitIds.Length; i++)
        {
            stream.Serialize(ref spawnedUnitIds[i]);
        }
    }
}