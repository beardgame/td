using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Audio;

sealed class NoOpSoundLoop : IPositionedSoundLoop
{
    public void MoveTo(Position3 position) {}
    public void Stop() {}
}
