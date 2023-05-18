using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Audio;

static class SoundScapeExtensions
{
    public static void PlayGlobalSound(this ISoundScape soundScape, ISoundEffect effect)
    {
        soundScape.PlayGlobalSound(effect.Sound, effect.Gain);
    }

    public static void PlaySoundAt(this ISoundScape soundScape, ISoundEffect effect, Position3 position)
    {
        soundScape.PlaySoundAt(effect.Sound, position, effect.Gain, effect.PitchRange.ChooseRandomPitch());
    }

    public static ISoundLoop LoopGlobalSound(this ISoundScape soundScape, ISoundEffect effect)
    {
        return soundScape.LoopGlobalSound(effect.Sound, effect.Gain);
    }

    public static IPositionedSoundLoop LoopSoundAt(this ISoundScape soundScape, ISoundEffect effect, Position3 position)
    {
        return soundScape.LoopSoundAt(effect.Sound, position, effect.Gain);
    }
}
