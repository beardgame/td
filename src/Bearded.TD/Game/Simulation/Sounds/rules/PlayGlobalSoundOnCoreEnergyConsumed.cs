using Bearded.TD.Audio;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Rules;

namespace Bearded.TD.Game.Simulation.Sounds;

[GameRule("playSoundOnCoreEnergyConsumed")]
sealed class PlayGlobalSoundOnCoreEnergyConsumed : PlayGlobalSoundOn<PlayGlobalSoundOnCoreEnergyConsumed.RuleParameters, ResourcesConsumed<CoreEnergy>>
{
    public record struct RuleParameters(ISoundEffect Sound, double Threshold = 10);

    protected override ISoundEffect SoundEffect => Parameters.Sound;

    public PlayGlobalSoundOnCoreEnergyConsumed(RuleParameters parameters) : base(parameters) { }

    protected override bool ShouldPlaySoundEffect(ResourcesConsumed<CoreEnergy> @event) =>
        @event.AmountConsumed.Value >= Parameters.Threshold;
}
