using Bearded.TD.Audio;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Shared.Events;

namespace Bearded.TD.Game.Simulation.Sounds;

[GameRule("loopGlobalSoundDuringWaves")]
sealed class LoopGlobalSoundDuringWaves :
    GameRule<LoopGlobalSoundDuringWaves.RuleParameters>, IListener<WaveStarted>, IListener<WaveEnded>
{
    public record struct RuleParameters(ISoundEffect Sound);

    public LoopGlobalSoundDuringWaves(RuleParameters parameters) : base(parameters) { }

    private ISoundScape? soundScape;
    private ISoundLoop? loop;

    public override void Execute(GameRuleContext context)
    {
        soundScape = context.GameState.Meta.SoundScape;
        context.Events.Subscribe<WaveStarted>(this);
        context.Events.Subscribe<WaveEnded>(this);
    }

    public void HandleEvent(WaveStarted @event)
    {
        loop ??= soundScape?.LoopGlobalSound(Parameters.Sound);
    }

    public void HandleEvent(WaveEnded @event)
    {
        loop?.Stop();
        loop = null;
    }
}
