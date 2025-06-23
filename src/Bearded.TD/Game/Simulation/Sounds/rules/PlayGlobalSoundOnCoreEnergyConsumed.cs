using Bearded.TD.Audio;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Simulation.Sounds.PlayGlobalSoundOnCoreEnergyConsumed;

namespace Bearded.TD.Game.Simulation.Sounds;

[GameRule("playSoundOnCoreEnergyConsumed")]
sealed class PlayGlobalSoundOnCoreEnergyConsumed(RuleParameters parameters)
    : PlayGlobalSoundOn<RuleParameters, ResourcesConsumed<CoreEnergy>>(parameters)
{
    public record struct RuleParameters(ISoundEffect Sound, double Threshold = 10);

    protected override ISoundEffect SoundEffect => Parameters.Sound;
    protected override TimeSpan Cooldown => TimeSpan.Zero;

    protected override bool ShouldPlaySoundEffect(ResourcesConsumed<CoreEnergy> @event) =>
        @event.AmountConsumed.Value >= Parameters.Threshold;
}
