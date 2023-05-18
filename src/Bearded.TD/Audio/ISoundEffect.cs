using Bearded.TD.Game.Simulation;

namespace Bearded.TD.Audio;

interface ISoundEffect : IBlueprint
{
    ISound Sound { get; }
    float Gain { get; }
    PitchRange PitchRange { get; }
}
