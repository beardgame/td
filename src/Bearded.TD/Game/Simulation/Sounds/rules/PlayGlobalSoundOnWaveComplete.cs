using Bearded.TD.Audio;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Rules;

namespace Bearded.TD.Game.Simulation.Sounds;

[GameRule("playSoundOnWaveComplete")]
sealed class PlayGlobalSoundOnWaveComplete : PlayGlobalSoundOn<PlayGlobalSoundOnWaveComplete.RuleParameters, WaveEnded>
{
    public record struct RuleParameters(ISoundEffect Sound);

    protected override ISoundEffect SoundEffect => Parameters.Sound;

    public PlayGlobalSoundOnWaveComplete(RuleParameters parameters) : base(parameters) { }
}
