using Bearded.TD.Audio;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Shared.Events;

namespace Bearded.TD.Game.Simulation.Sounds;

[GameRule("loopGlobalSound")]
sealed class LoopGlobalSound : GameRule<LoopGlobalSound.RuleParameters>, IListener<GameStarted>
{
    public record struct RuleParameters(ISoundEffect Sound);

    public LoopGlobalSound(RuleParameters parameters) : base(parameters) { }

    private ISoundScape? soundScape;
    private ISoundLoop? loop;

    public override void Execute(GameRuleContext context)
    {
        soundScape = context.GameState.Meta.SoundScape;
        context.Events.Subscribe(this);
    }

    public void HandleEvent(GameStarted @event)
    {
        loop = soundScape?.LoopGlobalSound(Parameters.Sound);
    }
}
