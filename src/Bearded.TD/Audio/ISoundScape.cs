using System;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Audio;

interface ISoundScape : IDisposable
{
    void Update();
    void SetListenerPosition(Position3 position);
    void PlaySoundAt(Position3 position, ISound sound, float? pitch);
}
