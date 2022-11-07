using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Audio;

interface ISoundLoop
{
    void MoveTo(Position3 position);
    void Stop();
}
