using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Enemies;
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
            enemyScript.ToSpawnScript(game),
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

        public EnemySpawnScript ToSpawnScript(GameInstance game) =>
            new(spawnEvents.Select(e => e!.ToSpawnEvent(game)).ToImmutableArray());
    }

    private sealed class EnemySpawnEventSerializer
    {
        private TimeSpan time;
        private readonly EnemyFormSerializer enemyForm = new();

        [UsedImplicitly]
        public EnemySpawnEventSerializer() {}

        public EnemySpawnEventSerializer(EnemySpawnScript.EnemySpawnEvent spawnEvent)
        {
            time = spawnEvent.TimeOffset;
            enemyForm = new EnemyFormSerializer(spawnEvent.EnemyForm);
        }

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref time);
            enemyForm.Serialize(stream);
        }

        public EnemySpawnScript.EnemySpawnEvent ToSpawnEvent(GameInstance game) =>
            new(time, enemyForm.ToEnemyForm(game));
    }

    private sealed class EnemyFormSerializer
    {
        private ModAwareId blueprint;
        private (SocketShape, ModAwareId)[] modules = Array.Empty<(SocketShape, ModAwareId)>();

        [UsedImplicitly]
        public EnemyFormSerializer() {}

        public EnemyFormSerializer(EnemyForm form)
        {
            blueprint = form.Blueprint.Id;
            modules = form.Modules.Select(kvp => (kvp.Key, kvp.Value.Id)).ToArray();
        }

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref blueprint);
            stream.SerializeArrayCount(ref modules);
            for (var i = 0; i < modules.Length; i++)
            {
                stream.Serialize(ref modules[i].Item1);
                stream.Serialize(ref modules[i].Item2);
            }
        }

        public EnemyForm ToEnemyForm(GameInstance game) =>
            new(game.Blueprints.GameObjects[blueprint], toModuleDictionary(game));

        private ImmutableDictionary<SocketShape, IModule> toModuleDictionary(GameInstance game) =>
            modules.ToImmutableDictionary(tuple => tuple.Item1, toTuple => game.Blueprints.Modules[toTuple.Item2]);
    }
}
