using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Simulation.Statistics.Serialization;

sealed class SerializableWaveReport
{
    private SerializableTowerStatistics?[] towers = [];

    public SerializableWaveReport() { }

    public SerializableWaveReport(WaveReport instance)
    {
        towers = instance.AllTowers.Select(t => new SerializableTowerStatistics(t)).ToArray();
    }

    public void Serialize(INetBufferStream stream)
    {
        stream.SerializeArrayCount(ref towers);
        for (var i = 0; i < towers.Length; i++)
        {
            var t = towers[i] ?? new SerializableTowerStatistics();
            t.Serialize(stream);
            towers[i] = t;
        }
    }

    public WaveReport ToInstance(GameInstance game)
    {
        return WaveReport.Create(towers.Select(t => t.ToInstance(game)).ToImmutableArray());
    }
}
