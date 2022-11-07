using Bearded.TD.Audio;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Rules;

namespace Bearded.TD.Game.Simulation.Sounds;

[GameRule("playSoundOnWaveStart")]
sealed class PlayGlobalSoundOnWaveStart : PlayGlobalSoundOn<PlayGlobalSoundOnWaveStart.RuleParameters, WaveStarted>
{
    public record struct RuleParameters(ISoundEffect Sound);

    protected override ISoundEffect SoundEffect => Parameters.Sound;

    public PlayGlobalSoundOnWaveStart(RuleParameters parameters) : base(parameters) { }
}
