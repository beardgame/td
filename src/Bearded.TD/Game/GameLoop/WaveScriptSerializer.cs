using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using JetBrains.Annotations;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.GameLoop;

sealed class WaveScriptSerializer
{
    private Id<WaveScript> id;
    private string displayName = "";
    private Id<Faction> targetFaction;
    private double spawnStart;
    private double spawnDuration;
    private int resourcesAwardedBySpawnPhase;
    private Id<SpawnLocation>[] spawnLocations = Array.Empty<Id<SpawnLocation>>();
    private readonly EnemySpawnScriptSerializer enemyScript = new();
    private ModAwareId unitBlueprint;
    private Id<GameObject>[] spawnedUnitIds = Array.Empty<Id<GameObject>>();

    [UsedImplicitly]
    public WaveScriptSerializer() {}

    public WaveScriptSerializer(WaveScript waveScript)
    {
        id = waveScript.Id;
        displayName = waveScript.DisplayName;
        targetFaction = waveScript.TargetFaction.Id;
        spawnStart = waveScript.SpawnStart?.NumericValue ?? -1;
        spawnDuration = waveScript.SpawnDuration.NumericValue;
        resourcesAwardedBySpawnPhase = waveScript.ResourcesAwarded.NumericValue;
        spawnLocations = waveScript.SpawnLocations.Select(loc => loc.Id).ToArray();
        enemyScript = new EnemySpawnScriptSerializer(waveScript.EnemyScript);
        unitBlueprint = waveScript.UnitBlueprint.Id;
        spawnedUnitIds = waveScript.SpawnedUnitIds.ToArray();
    }

    public WaveScript ToWaveScript(GameInstance game)
    {
        return new WaveScript(
            id,
            displayName,
            game.State.Factions.Resolve(targetFaction),
            spawnStart < 0 ? null : new Instant(spawnStart),
            new TimeSpan(spawnDuration),
            new ResourceAmount(resourcesAwardedBySpawnPhase),
            spawnLocations.Select(loc => game.State.Find(loc)).ToImmutableArray(),
            enemyScript.ToSpawnScript(),
            game.Blueprints.GameObjects[unitBlueprint],
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
        enemyScript.Serialize(stream);
        stream.Serialize(ref unitBlueprint);
        stream.SerializeArrayCount(ref spawnedUnitIds);
        for (var i = 0; i < spawnedUnitIds.Length; i++)
        {
            stream.Serialize(ref spawnedUnitIds[i]);
        }
    }

    private sealed class EnemySpawnScriptSerializer
    {
        private EnemySpawnEventSerializer?[] spawnEvents = Array.Empty<EnemySpawnEventSerializer>();

        [UsedImplicitly]
        public EnemySpawnScriptSerializer() {}

        public EnemySpawnScriptSerializer(EnemySpawnScript script)
        {
            spawnEvents = script.SpawnEvents.Select(e => new EnemySpawnEventSerializer(e)).ToArray();
        }

        public void Serialize(INetBufferStream stream)
        {
            stream.SerializeArrayCount(ref spawnEvents);
            for (var i = 0; i < spawnEvents.Length; i++)
            {
                var serializer = spawnEvents[i] ?? new EnemySpawnEventSerializer();
                serializer.Serialize(stream);
                spawnEvents[i] = serializer;
            }
        }

        public EnemySpawnScript ToSpawnScript() => new(spawnEvents.Select(e => e!.ToSpawnEvent()).ToImmutableArray());
    }

    private sealed class EnemySpawnEventSerializer
    {
        private TimeSpan time;

        [UsedImplicitly]
        public EnemySpawnEventSerializer() {}

        public EnemySpawnEventSerializer(EnemySpawnScript.EnemySpawnEvent spawnEvent)
        {
            time = spawnEvent.TimeOffset;
        }

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref time);
        }

        public EnemySpawnScript.EnemySpawnEvent ToSpawnEvent() => new(time);
    }
}
