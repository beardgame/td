using System;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Audio;

interface ISoundScape : IDisposable
{
    void Update();
    void SetListenerPosition(Position3 position);
    void PlayGlobalSound(ISound sound);
    void PlaySoundAt(ISound sound, Position3 position, float? pitch = null);
    ISoundLoop LoopSoundAt(ISound sound, Position3 position);
}
