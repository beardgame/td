using Bearded.TD.Audio;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Simulation.Sounds;

[Component("playSoundOnDamage")]
sealed class PlaySoundOnDamage : PlaySoundOn<PlaySoundOnDamage.IParameters, TakeDamage>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        ISoundEffect Sound { get; }
    }

    protected override ISoundEffect SoundEffect => Parameters.Sound;

    public PlaySoundOnDamage(IParameters parameters) : base(parameters) { }
}
