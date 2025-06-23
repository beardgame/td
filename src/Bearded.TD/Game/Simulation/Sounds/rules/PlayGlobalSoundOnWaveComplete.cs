using Bearded.TD.Audio;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Simulation.Sounds.PlayGlobalSoundOnWaveComplete;

namespace Bearded.TD.Game.Simulation.Sounds;

[GameRule("playSoundOnWaveComplete")]
sealed class PlayGlobalSoundOnWaveComplete(RuleParameters parameters)
    : PlayGlobalSoundOn<RuleParameters, WaveEnded>(parameters)
{
    public record struct RuleParameters(ISoundEffect Sound);

    protected override ISoundEffect SoundEffect => Parameters.Sound;
    protected override TimeSpan Cooldown => TimeSpan.Zero;
}
