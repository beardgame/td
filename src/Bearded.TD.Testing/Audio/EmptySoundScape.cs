using Bearded.TD.Audio;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Testing.Audio;

sealed class EmptySoundScape : ISoundScape
{
    public void Update() {}

    public void SetListenerPosition(Position3 position) {}

    public void PlayGlobalSound(ISound sound, float? gain) {}

    public void PlaySoundAt(ISound sound, Position3 position, float? gain, float? pitch) {}

    public ISoundLoop LoopGlobalSound(ISound sound, float? gain) => new NoOpSoundLoop();

    public IPositionedSoundLoop LoopSoundAt(ISound sound, Position3 position, float? gain) => new NoOpSoundLoop();

    public void Dispose() {}
}
