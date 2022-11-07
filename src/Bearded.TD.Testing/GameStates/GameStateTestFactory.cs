using Bearded.TD.Game;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Testing.Audio;
using Bearded.Utilities;
using Bearded.Utilities.IO;

namespace Bearded.TD.Testing.GameStates;

static class GameStateTestFactory
{
    public static GameState Create()
    {
        var logger = new Logger();
        var meta = new GameMeta(
            logger,
            new LocalDispatcher(),
            new NoOpSynchronizer(),
            new IdManager(),
            new NoOpSpriteRenderers(),
            new EmptySoundScape());
        var settings = new GameSettings.Builder().Build();
        var state = new GameState(meta, settings);

        state.FinishLoading();

        return state;
    }
}
