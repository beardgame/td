using Bearded.TD.Audio;
using Bearded.TD.Game.Simulation.Buildings.Veterancy;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Simulation.Sounds;

[Component("playSoundOnLevelGain")]
sealed class PlaySoundOnLevelGain : PlaySoundOn<PlaySoundOnLevelGain.IParameters, GainLevel>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        ISoundEffect Sound { get; }
    }

    protected override ISoundEffect SoundEffect => Parameters.Sound;

    public PlaySoundOnLevelGain(IParameters parameters) : base(parameters) { }
}
