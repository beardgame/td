using Bearded.TD.Audio;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Rules;
using static Bearded.TD.Game.Simulation.Sounds.PlayGlobalSoundOnScrapConsumed;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Sounds;

[GameRule("playSoundOnScrapConsumed")]
sealed class PlayGlobalSoundOnScrapConsumed(RuleParameters parameters)
    : PlayGlobalSoundOn<RuleParameters, ResourcesConsumed<Scrap>>(parameters)
{
    public record struct RuleParameters(ISoundEffect Sound);

    protected override ISoundEffect SoundEffect => Parameters.Sound;
    protected override TimeSpan Cooldown => TimeSpan.Zero;
}
