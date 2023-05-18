using System;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Audio;

interface ISoundScape : IDisposable
{
    void Update();
    void SetListenerPosition(Position3 position);
    void PlayGlobalSound(ISound sound, float? gain = null);
    void PlaySoundAt(ISound sound, Position3 position, float? gain = null, float? pitch = null);
    ISoundLoop LoopGlobalSound(ISound sound, float? gain = null);
    IPositionedSoundLoop LoopSoundAt(ISound sound, Position3 position, float? gain = null);
}
