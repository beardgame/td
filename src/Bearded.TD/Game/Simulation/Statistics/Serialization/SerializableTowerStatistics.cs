using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Statistics.Data;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Statistics.Serialization;

sealed class SerializableTowerStatistics
{
    private Id<GameObject> id;
    private SerializableTypedAccumulatedDamage?[] damageByType = [];

    public SerializableTowerStatistics() { }

    public SerializableTowerStatistics(TowerStatistics instance)
    {
        id = instance.Metadata.Id;
        damageByType = instance.DamageByType.Select(d => new SerializableTypedAccumulatedDamage(d)).ToArray();
    }

    public void Serialize(INetBufferStream stream)
    {
        stream.Serialize(ref id);
        stream.SerializeArrayCount(ref damageByType);
        for (var i = 0; i < damageByType.Length; i++)
        {
            var d = damageByType[i] ?? new SerializableTypedAccumulatedDamage();
            d.Serialize(stream);
            damageByType[i] = d;
        }
    }

    public TowerStatistics ToInstance(GameInstance game)
    {
        var metadata = game.State.Statistics.FindTowerMetadata(id);
        return new TowerStatistics(metadata, damageByType.Select(d => d!.ToInstance()).ToImmutableArray());
    }
}
