using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Shared.Events;

namespace Bearded.TD.Game.Simulation.Exploration;

[GameRule("revealFullMap")]
sealed class RevealFullMap : GameRule
{
    public override void Execute(GameRuleContext context)
    {
        context.Events.Subscribe(new Listener(context.GameState));
    }

    private sealed class Listener : IListener<GameStarted>
    {
        private readonly GameState gameState;

        public Listener(GameState gameState)
        {
            this.gameState = gameState;
        }

        public void HandleEvent(GameStarted @event)
        {
            gameState.VisibilityLayer.RevealAllZones();
        }
    }
}