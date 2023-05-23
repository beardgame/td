using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Game.GameLoop;

sealed class SpawnLocationActivator
{
    private readonly GameState game;
    private readonly ICommandDispatcher<GameInstance> commandDispatcher;
    private readonly Random random;

    public SpawnLocationActivator(GameState game, ICommandDispatcher<GameInstance> commandDispatcher, int seed)
    {
        this.game = game;
        this.commandDispatcher = commandDispatcher;
        random = new Random(seed);
    }

    public void WakeSpawnLocation()
    {
        var dormantSpawnLocations = game.Enumerate<SpawnLocation>().Where(s => !s.IsAwake).ToImmutableArray();
        if (dormantSpawnLocations.Length > 0)
        {
            commandDispatcher.Dispatch(WakeUpSpawnLocation.Command(dormantSpawnLocations.RandomElement(random)));
        }
    }

    public ImmutableArray<SpawnLocation> GatherAvailableSpawnLocations()
    {
        return game.Enumerate<SpawnLocation>().Where(s => s.IsAwake).ToImmutableArray();
    }
}
