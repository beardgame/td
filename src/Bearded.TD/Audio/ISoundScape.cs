using System;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Audio;

interface ISoundScape : IDisposable
{
    void Update();
    void SetListenerPosition(Position3 position);
    void PlaySoundAt(ISound sound, Position3 position, float? pitch);
    ISoundLoop LoopSoundAt(ISound sound, Position3 position);
}
