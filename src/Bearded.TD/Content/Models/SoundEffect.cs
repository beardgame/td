using Bearded.TD.Audio;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation;

namespace Bearded.TD.Content.Models;

sealed class SoundEffect : IBlueprint
{
    public ModAwareId Id { get; }
    public ISound Sound { get; }

    public SoundEffect(ModAwareId id, ISound sound)
    {
        Id = id;
        Sound = sound;
    }
}
