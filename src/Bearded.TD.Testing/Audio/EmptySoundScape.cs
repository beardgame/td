using Bearded.TD.Audio;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Testing.Audio;

sealed class EmptySoundScape : ISoundScape
{
    public void Update() {}

    public void SetListenerPosition(Position3 position) {}

    public void PlayGlobalSound(ISound sound) {}

    public void PlaySoundAt(ISound sound, Position3 position, float? pitch) {}

    public ISoundLoop LoopSoundAt(ISound sound, Position3 position) => new NoOpSoundLoop();

    public void Dispose() {}
}
