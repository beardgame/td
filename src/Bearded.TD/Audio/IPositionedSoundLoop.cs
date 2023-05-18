using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Audio;

interface IPositionedSoundLoop : ISoundLoop
{
    void MoveTo(Position3 position);
}
