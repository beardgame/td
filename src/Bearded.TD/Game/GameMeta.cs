using System;
using Bearded.TD.Audio;
using Bearded.TD.Commands;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using Bearded.Utilities.IO;

namespace Bearded.TD.Game;

sealed class GameMeta
{
    private Blueprints? blueprints;

    public IDispatcher<GameInstance> Dispatcher { get; }
    public IGameSynchronizer Synchronizer { get; }
    public IdManager Ids { get; }
    public Logger Logger { get; }
    public bool GameOver { get; private set; }
    public GlobalGameEvents Events { get; } = new();
    public Blueprints Blueprints => blueprints!;
    public ISoundScape SoundScape { get; }

    public ISpriteRenderers SpriteRenderers { get; }

    public GameMeta(
        Logger logger,
        IDispatcher<GameInstance> dispatcher,
        IGameSynchronizer synchronizer,
        IdManager ids,
        ISpriteRenderers spriteRenderers,
        ISoundScape soundScape)
    {
        Logger = logger;
        Synchronizer = synchronizer;
        Dispatcher = dispatcher;
        Ids = ids;
        SpriteRenderers = spriteRenderers;
        SoundScape = soundScape;
    }

    public void SetBlueprints(Blueprints blueprints)
    {
        // TODO: remove this mutability.
        // Might have to initialise the Meta class late in the GameInstance,
        // but that will require some refactoring

        if (this.blueprints != null)
            throw new InvalidOperationException("Can only set blueprints once.");

        this.blueprints = blueprints;
    }

    public void DoGameOver()
    {
        if (GameOver)
        {
            return;
        }
        GameOver = true;
        Events.Send(new GameOverTriggered());
    }

    public void DoGameVictory()
    {
        if (GameOver)
        {
            return;
        }
        GameOver = true;
        Events.Send(new GameVictoryTriggered());
    }
}
