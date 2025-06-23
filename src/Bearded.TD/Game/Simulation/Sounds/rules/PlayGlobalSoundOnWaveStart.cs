using Bearded.TD.Audio;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Simulation.Sounds.PlayGlobalSoundOnWaveStart;

namespace Bearded.TD.Game.Simulation.Sounds;

[GameRule("playSoundOnWaveStart")]
sealed class PlayGlobalSoundOnWaveStart(RuleParameters parameters)
    : PlayGlobalSoundOn<RuleParameters, WaveStarted>(parameters)
{
    public record struct RuleParameters(ISoundEffect Sound);

    protected override ISoundEffect SoundEffect => Parameters.Sound;
    protected override TimeSpan Cooldown => TimeSpan.Zero;
}
