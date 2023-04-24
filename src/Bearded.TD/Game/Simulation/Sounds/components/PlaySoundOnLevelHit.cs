using Bearded.TD.Audio;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Simulation.Sounds;

[Component("playSoundOnLevelHit")]
sealed class PlaySoundOnLevelHit : PlaySoundOn<PlaySoundOnLevelHit.IParameters, CollideWithLevel>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        ISoundEffect Sound { get; }
    }

    protected override ISoundEffect SoundEffect => Parameters.Sound;

    public PlaySoundOnLevelHit(IParameters parameters) : base(parameters) { }
}
