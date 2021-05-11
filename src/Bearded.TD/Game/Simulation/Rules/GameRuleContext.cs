using System.Collections.ObjectModel;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Rules
{
    sealed class GameRuleContext
    {
        public GameState GameState { get; }
        public GlobalGameEvents Events { get; }
        public GameSettings GameSettings => GameState.GameSettings;
        public ReadOnlyCollection<Faction> Factions => GameState.Factions;
        public IDispatcher<GameInstance> Dispatcher => GameState.Meta.Dispatcher;
        public IdManager Ids => GameState.Meta.Ids;

        public Faction RootFaction => Factions.First(f => f.Parent == null);

        public GameRuleContext(GameState gameState, GlobalGameEvents events)
        {
            Events = events;
            GameState = gameState;
        }
    }
}
