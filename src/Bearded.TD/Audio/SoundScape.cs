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

    public void PlayGlobalSound(ISound sound, float? gain)
    {
        if (!sourcePool.TryGetSource(out var source))
        {
            return;
        }

        playSound(source, sound, Position3.Zero, gain: gain, positionIsRelative: true);
    }

    public void PlaySoundAt(ISound sound, Position3 position, float? gain, float? pitch)
    {
        if (!sourcePool.TryGetSource(out var source))
        {
            return;
        }

        playSound(source, sound, position, gain: gain, pitch: pitch);
    }

    public ISoundLoop LoopGlobalSound(ISound sound, float? gain)
    {
        if (!sourcePool.TryGetSource(out var source))
        {
            return new NoOpSoundLoop();
        }

        var instance = playSound(source, sound, Position3.Zero, gain: gain, positionIsRelative: true, looping: true);
        return new SoundLoop(this, instance);
    }

    public IPositionedSoundLoop LoopSoundAt(ISound sound, Position3 position, float? gain)
    {
        if (!sourcePool.TryGetSource(out var source))
        {
            return new NoOpSoundLoop();
        }

        var instance = playSound(source, sound, position, gain: gain, looping: true);
        return new PositionedSoundLoop(this, instance);
    }

    private SoundInstance playSound(
        Source source,
        ISound sound,
        Position3 position,
        bool looping = false,
        float? gain = null,
        float? pitch = null,
        bool positionIsRelative = false)
    {
        configureSource(source, position, looping, gain, pitch, positionIsRelative);

        var buffer = sound.ToBuffer();
        source.QueueBuffer(buffer);
        source.Play();

        var instance = new SoundInstance(source, buffer);
        soundInstances.Add(instance);
        return instance;
    }

    private static void configureSource(
        Source source, Position3 position, bool looping, float? gain, float? pitch, bool positionIsRelative)
    {
        source.Position = position.NumericValue;
        source.PositionIsRelative = positionIsRelative;
        source.Looping = looping;
        source.Gain = gain ?? 1;
        source.Pitch = pitch ?? 1;
    }

    public void Dispose()
    {
        soundInstances.Clear();
        sourcePool.Dispose();
    }

    public static SoundScape WithChannelCount(int numChannels)
    {
        var sourcePool = SourcePool.CreateInstanceAndAllocateSources(numChannels);
        return new SoundScape(sourcePool);
    }

    private class SoundLoop : ISoundLoop
    {
        private readonly SoundScape soundScape;
        protected SoundInstance Sound { get; }

        public SoundLoop(SoundScape soundScape, SoundInstance sound)
        {
            this.soundScape = soundScape;
            Sound = sound;
        }

        public void Stop()
        {
            Sound.Source.Stop();
            soundScape.disposeSound(Sound);
        }
    }

    private sealed class PositionedSoundLoop : SoundLoop, IPositionedSoundLoop
    {
        public PositionedSoundLoop(SoundScape soundScape, SoundInstance sound) : base(soundScape, sound) { }

        public void MoveTo(Position3 position)
        {
            Sound.Source.Position = position.NumericValue;
        }
    }

    private readonly record struct SoundInstance(Source Source, SoundBuffer Buffer);
}
