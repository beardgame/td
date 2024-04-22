using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.GameLoop;

sealed class WaveScriptSerializer
{
    private string displayName = "";
    private Id<Faction> targetFaction;
    private double spawnStart;
    private Id<SpawnLocation>[] spawnLocations = Array.Empty<Id<SpawnLocation>>();
    private readonly EnemySpawnScriptSerializer enemyScript = new();

    [UsedImplicitly]
    public WaveScriptSerializer() {}

    public WaveScriptSerializer(WaveScript waveScript)
    {
        displayName = waveScript.DisplayName;
        targetFaction = waveScript.TargetFaction.Id;
        spawnStart = waveScript.DowntimeDuration?.NumericValue ?? -1;
        spawnLocations = waveScript.SpawnLocations.Select(loc => loc.Id).ToArray();
        enemyScript = new EnemySpawnScriptSerializer(waveScript.EnemyScript);
    }

    public WaveScript ToWaveScript(GameInstance game)
    {
        return new WaveScript(
            displayName,
            game.State.Factions.Resolve(targetFaction),
            spawnStart < 0 ? null : new TimeSpan(spawnStart),
            spawnLocations.Select(loc => game.State.Find(loc)).ToImmutableArray(),
            enemyScript.ToSpawnScript(game));
    }

    public void Serialize(INetBufferStream stream)
    {
        stream.Serialize(ref displayName);
        stream.Serialize(ref targetFaction);
        stream.Serialize(ref spawnStart);
        stream.SerializeArrayCount(ref spawnLocations);
        for (var i = 0; i < spawnLocations.Length; i++)
        {
            stream.Serialize(ref spawnLocations[i]);
        }
        enemyScript.Serialize(stream);
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
        private Id<SpawnLocation> spawnLocation;
        private TimeSpan time;
        private readonly EnemyFormSerializer enemyForm = new();

        [UsedImplicitly]
        public EnemySpawnEventSerializer() {}

        public EnemySpawnEventSerializer(EnemySpawnScript.EnemySpawnEvent spawnEvent)
        {
            spawnLocation = spawnEvent.SpawnLocation.Id;
            time = spawnEvent.TimeOffset;
            enemyForm = new EnemyFormSerializer(spawnEvent.EnemyForm);
        }

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref spawnLocation);
            stream.Serialize(ref time);
            enemyForm.Serialize(stream);
        }

        public EnemySpawnScript.EnemySpawnEvent ToSpawnEvent(GameInstance game) =>
            new(game.State.Find(spawnLocation), time, enemyForm.ToEnemyForm(game));
    }

    private sealed class EnemyFormSerializer
    {
        private ModAwareId blueprint;
        private (string, ModAwareId)[] modules = Array.Empty<(string, ModAwareId)>();
        private (DamageType, float)[] resistances = Array.Empty<(DamageType, float)>();

        [UsedImplicitly]
        public EnemyFormSerializer() {}

        public EnemyFormSerializer(EnemyForm form)
        {
            blueprint = form.Blueprint.Id;
            modules = form.Modules.Select(kvp => (kvp.Key.Id, kvp.Value.Id)).ToArray();
            resistances = form.Resistances.Select(kvp => (kvp.Key, kvp.Value.NumericValue)).ToArray();
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
            stream.SerializeArrayCount(ref resistances);
            for (var i = 0; i < resistances.Length; i++)
            {
                stream.Serialize(ref resistances[i].Item1);
                stream.Serialize(ref resistances[i].Item2);
            }
        }

        public EnemyForm ToEnemyForm(GameInstance game) =>
            new(game.Blueprints.GameObjects[blueprint], toModuleDictionary(game), toResistanceDictionary());

        private ImmutableDictionary<SocketShape, IModule> toModuleDictionary(GameInstance game) =>
            modules.ToImmutableDictionary(tuple =>
                SocketShape.FromLiteral(tuple.Item1), toTuple => game.Blueprints.Modules[toTuple.Item2]);

        private ImmutableDictionary<DamageType, Resistance> toResistanceDictionary() =>
            resistances.ToImmutableDictionary(tuple => tuple.Item1, tuple => new Resistance(tuple.Item2));
    }
}
