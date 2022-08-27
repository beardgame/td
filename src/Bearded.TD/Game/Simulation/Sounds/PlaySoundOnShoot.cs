using Bearded.TD.Audio;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Simulation.Sounds;

[Component("playSoundOnShoot")]
sealed class PlaySoundOnShoot : PlaySoundOn<PlaySoundOnShoot.IParameters, ShotProjectile>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        ISoundEffect Sound { get; }
    }

    protected override ISoundEffect SoundEffect => Parameters.Sound;

    public PlaySoundOnShoot(IParameters parameters) : base(parameters) { }
}
