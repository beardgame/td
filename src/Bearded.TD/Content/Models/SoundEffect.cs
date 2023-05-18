using Bearded.TD.Audio;
using Bearded.TD.Content.Mods;

namespace Bearded.TD.Content.Models;

sealed class SoundEffect : ISoundEffect
{
    public ModAwareId Id { get; }
    public ISound Sound { get; }
    public float Gain { get; }
    public PitchRange PitchRange { get; }

    public SoundEffect(ModAwareId id, ISound sound, float gain, PitchRange pitchRange)
    {
        Id = id;
        Sound = sound;
        Gain = gain;
        PitchRange = pitchRange;
    }
}
