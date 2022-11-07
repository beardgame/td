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
            disposeSound(sound);
        }
    }

    private void disposeSound(SoundInstance sound)
    {
        // Can only reclaim finished sources, so put the source in a finished state, even if it isn't finished.
        sound.Source.DequeueBuffers();
        sound.Source.Looping = false;

        sourcePool.ReclaimSource(sound.Source);
        sound.Buffer.Dispose();
        soundInstances.Remove(sound);
    }

    public void SetListenerPosition(Position3 position)
    {
        ALListener.Position = position.NumericValue;
    }

    public void PlaySoundAt(ISound sound, Position3 position, float? pitch)
    {
        if (!sourcePool.TryGetSource(out var source))
        {
            return;
        }

        playSound(source, sound, position, pitch: pitch);
    }

    public ISoundLoop LoopSoundAt(ISound sound, Position3 position)
    {
        if (!sourcePool.TryGetSource(out var source))
        {
            return new NoOpSoundLoop();
        }

        var instance = playSound(source, sound, position, looping: true);
        return new SoundLoop(this, instance);
    }

    private SoundInstance playSound(Source source, ISound sound, Position3 position, bool looping = false, float? pitch = null)
    {
        configureSource(source, position, looping, pitch);

        var buffer = sound.ToBuffer();
        source.QueueBuffer(buffer);
        source.Play();

        var instance = new SoundInstance(source, buffer);
        soundInstances.Add(instance);
        return instance;
    }

    private static void configureSource(Source source, Position3 position, bool looping, float? pitch)
    {
        source.Position = position.NumericValue;
        source.PositionIsRelative = false;
        source.Looping = looping;
        source.Pitch = pitch ?? 1;
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

    private sealed class SoundLoop : ISoundLoop
    {
        private readonly SoundScape soundScape;
        private readonly SoundInstance sound;

        public SoundLoop(SoundScape soundScape, SoundInstance sound)
        {
            this.soundScape = soundScape;
            this.sound = sound;
        }

        public void MoveTo(Position3 position)
        {
            sound.Source.Position = position.NumericValue;
        }

        public void Stop()
        {
            sound.Source.Stop();
            soundScape.disposeSound(sound);
        }
    }

    private readonly record struct SoundInstance(Source Source, SoundBuffer Buffer);
}
