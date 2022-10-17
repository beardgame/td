using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Audio;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Audio;

sealed class SoundScape : ISoundScape
{
    private readonly SourcePool sourcePool;
    private readonly List<Source> sourcesInUse = new();

    private SoundScape(SourcePool sourcePool)
    {
        this.sourcePool = sourcePool;
    }

    public void Update()
    {
        var finishedSources = sourcesInUse.Where(s => s.FinishedPlaying).ToImmutableArray();
        foreach (var source in finishedSources)
        {
            sourcePool.ReclaimSource(source);
            sourcesInUse.Remove(source);
        }
    }

    public void SetListenerPosition(Position3 position)
    {
        ALListener.Position = position.NumericValue;
    }

    public void PlaySoundAt(Position3 position, ISound sound, float? pitch)
    {
        if (!sourcePool.TryGetSource(out var source))
        {
            return;
        }

        source.Position = position.NumericValue;
        source.PositionIsRelative = false;
        source.Pitch = pitch ?? 1;
        var buffer = sound.ToBuffer();
        source.QueueBuffer(buffer);
        source.Play();
        sourcesInUse.Add(source);
    }

    public void Dispose()
    {
        sourcePool.Dispose();
    }

    public static SoundScape WithChannelCount(int numChannels)
    {
        var sourcePool = SourcePool.CreateInstanceAndAllocateSources(numChannels);
        return new SoundScape(sourcePool);
    }
}
