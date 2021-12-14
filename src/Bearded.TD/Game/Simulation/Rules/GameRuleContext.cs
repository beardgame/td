using System.Collections.ObjectModel;
using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.Utilities;
using Bearded.Utilities.IO;

namespace Bearded.TD.Game.Simulation.Rules;

sealed class GameRuleContext
{
    public Logger Logger { get; }
    public GameState GameState { get; }
    public GlobalGameEvents Events { get; }
    public ReadOnlyCollection<Player> Players { get; }
    public Blueprints Blueprints { get; }
    public GameSettings GameSettings => GameState.GameSettings;
    public IGameFactions Factions => GameState.Factions;
    public IDispatcher<GameInstance> Dispatcher => GameState.Meta.Dispatcher;
    public IdManager Ids => GameState.Meta.Ids;

    public GameRuleContext(
        GameState gameState, GlobalGameEvents events, ReadOnlyCollection<Player> players, Blueprints blueprints)
    {
        Logger = gameState.Meta.Logger;
        GameState = gameState;
        Events = events;
        Players = players;
        Blueprints = blueprints;
    }
}