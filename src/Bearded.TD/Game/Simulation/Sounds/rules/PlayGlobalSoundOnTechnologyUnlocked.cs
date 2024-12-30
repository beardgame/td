using Bearded.TD.Audio;
using Bearded.TD.Game.Simulation.Model;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Shared.Events;

namespace Bearded.TD.Game.Simulation.Sounds;

[GameRule("playSoundOnTechnologyUnlocked")]
sealed class PlayGlobalSoundOnTechnologyUnlocked : GameRule
{
    public override void Execute(GameRuleContext context)
    {
        context.Events.Subscribe(new Listener(context.GameState.Meta.SoundScape, context.GameState.Meta.Blueprints));
    }

    private sealed class Listener(ISoundScape soundScape, Blueprints blueprints) : IListener<TechnologyUnlocked>
    {
        public void HandleEvent(TechnologyUnlocked @event)
        {
            var soundEffect = @event.Technology.Branch.ToElement().GetUpgradeSound(blueprints);
            soundScape.PlayGlobalSound(soundEffect);
        }
    }
}
