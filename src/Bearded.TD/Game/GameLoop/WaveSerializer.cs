using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using JetBrains.Annotations;

namespace Bearded.TD.Game.GameLoop;

sealed class WaveSerializer
{
    private Id<Wave> id;
    private readonly WaveScriptSerializer scriptSerializer;
    private Id<GameObject>[] spawnedObjectIds;
    private double downtimeStart;

    [UsedImplicitly]
    public WaveSerializer()
    {
        scriptSerializer = new WaveScriptSerializer();
        spawnedObjectIds = Array.Empty<Id<GameObject>>();
    }

    public WaveSerializer(Wave wave)
    {
        id = wave.Id;
        scriptSerializer = new WaveScriptSerializer(wave.Script);
        spawnedObjectIds = wave.SpawnedObjectIds.ToArray();
        downtimeStart = wave.DowntimeStart.NumericValue;
    }

    public Wave ToWave(GameInstance game)
    {
        return new Wave(
            id,
            scriptSerializer.ToWaveScript(game),
            spawnedObjectIds.ToImmutableArray(),
            new Instant(downtimeStart));
    }

    public void Serialize(INetBufferStream stream)
    {
        stream.Serialize(ref id);
        scriptSerializer.Serialize(stream);
        stream.SerializeArrayCount(ref spawnedObjectIds);
        for (var i = 0; i < spawnedObjectIds.Length; i++)
        {
            stream.Serialize(ref spawnedObjectIds[i]);
        }
        stream.Serialize(ref downtimeStart);
    }
}
