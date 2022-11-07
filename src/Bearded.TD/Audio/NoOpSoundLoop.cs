using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Audio;

sealed class NoOpSoundLoop : ISoundLoop
{
    public void MoveTo(Position3 position) {}
    public void Stop() {}
}
