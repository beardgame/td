using Bearded.TD.Audio;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Rules;

namespace Bearded.TD.Game.Simulation.Sounds;

[GameRule("playSoundOnResourceGain")]
sealed class PlayGlobalSoundOnResourceGain : PlayGlobalSoundOn<PlayGlobalSoundOnResourceGain.RuleParameters, ResourcesProvided<CoreEnergy>>
{
    public record struct RuleParameters(ISoundEffect Sound);

    protected override ISoundEffect SoundEffect => Parameters.Sound;

    public PlayGlobalSoundOnResourceGain(RuleParameters parameters) : base(parameters) { }
}
