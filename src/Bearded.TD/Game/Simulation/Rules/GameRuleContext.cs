using System.Collections.ObjectModel;
using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Rules
{
    sealed class GameRuleContext
    {
        public GameState GameState { get; }
        public GlobalGameEvents Events { get; }
        public ReadOnlyCollection<Player> Players { get; }
        public GameSettings GameSettings => GameState.GameSettings;
        public IGameFactions Factions => GameState.Factions;
        public IDispatcher<GameInstance> Dispatcher => GameState.Meta.Dispatcher;
        public IdManager Ids => GameState.Meta.Ids;

        public GameRuleContext(GameState gameState, GlobalGameEvents events, ReadOnlyCollection<Player> players)
        {
            GameState = gameState;
            Events = events;
            Players = players;
        }
    }
}
