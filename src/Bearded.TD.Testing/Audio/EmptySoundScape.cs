using Bearded.TD.Audio;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Testing.Audio;

sealed class EmptySoundScape : ISoundScape
{
    public void Update() {}

    public void SetListenerPosition(Position3 position) {}

    public void PlaySoundAt(ISound sound, Position3 position, float? pitch) {}

    public void Dispose() {}
}
