using Bearded.TD.Audio;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Testing.Audio;

sealed class EmptySoundScape : ISoundScape
{
    public void Update() {}

    public void SetListenerPosition(Position3 position) {}

    public void PlaySoundAt(Position3 position, ISound sound, float? pitch) {}

    public void Dispose() {}
}
