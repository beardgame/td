using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.World;

namespace Bearded.TD.Game.Generation;

static class LevelGenerationParametersFactory
{
    public static LevelGenerationParameters Create(GameInstance game)
    {
        var nodeAccumulator = new Accumulator<NodeGroup>();
        game.Meta.Events.Send(new AccumulateNodeGroups(nodeAccumulator));
        var nodes = nodeAccumulator.Consume(toNodeGroup);

        var biomeAccumulator = new Accumulator<IBiome>();
        game.Meta.Events.Send(new AccumulateBiomes(biomeAccumulator));
        var biomes = biomeAccumulator.Consume(all => all.ToImmutableArray());

        return new LevelGenerationParameters(game.GameSettings.LevelSize, nodes, biomes);
    }

    private static NodeGroup toNodeGroup(IEnumerable<NodeGroup> nodeGroups)
    {
        var nodeGroupArray = nodeGroups.ToImmutableArray();
        if (nodeGroupArray.Length == 0)
        {
            throw new InvalidDataException("No nodes were contributed in the game rules.");
        }

        return new NodeGroup.Composite(nodeGroupArray, new NodeGroup.RandomizedNumber(1, null, null));
    }
}
