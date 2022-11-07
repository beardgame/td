using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Audio;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Audio;

sealed class SoundScape : ISoundScape
{
    private readonly SourcePool sourcePool;
    private readonly List<SoundInstance> soundInstances = new();

    private SoundScape(SourcePool sourcePool)
    {
        this.sourcePool = sourcePool;
    }

    public void Update()
    {
        var finishedSounds = soundInstances.Where(s => s.Source.FinishedPlaying).ToImmutableArray();
        foreach (var sound in finishedSounds)
        {
            sourcePool.ReclaimSource(sound.Source);
            sound.Buffer.Dispose();
            soundInstances.Remove(sound);
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
        soundInstances.Add(new SoundInstance(source, buffer));
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

    private readonly record struct SoundInstance(Source Source, SoundBuffer Buffer);
}
