using Bearded.TD.Audio;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Rules;

namespace Bearded.TD.Game.Simulation.Sounds;

[GameRule("playSoundOnScrapConsumed")]
sealed class PlayGlobalSoundOnScrapConsumed : PlayGlobalSoundOn<PlayGlobalSoundOnScrapConsumed.RuleParameters, ResourcesConsumed<Scrap>>
{
    public record struct RuleParameters(ISoundEffect Sound);

    protected override ISoundEffect SoundEffect => Parameters.Sound;

    public PlayGlobalSoundOnScrapConsumed(RuleParameters parameters) : base(parameters) { }
}
