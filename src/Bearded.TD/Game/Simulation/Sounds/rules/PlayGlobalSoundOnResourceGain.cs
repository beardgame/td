using Bearded.TD.Audio;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Simulation.Sounds.PlayGlobalSoundOnResourceGain;

namespace Bearded.TD.Game.Simulation.Sounds;

[GameRule("playSoundOnResourceGain")]
sealed class PlayGlobalSoundOnResourceGain(RuleParameters parameters)
    : PlayGlobalSoundOn<RuleParameters, ResourcesProvided<CoreEnergy>>(parameters)
{
    public record struct RuleParameters(ISoundEffect Sound);

    protected override ISoundEffect SoundEffect => Parameters.Sound;
    protected override TimeSpan Cooldown => TimeSpan.Zero;
}
