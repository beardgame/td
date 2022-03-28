using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation.Semantic.Commands;

sealed class LevelGenerationCommandAccumulator
{
    private readonly List<CommandFactory> commands = new();

    public void PlaceSpawnLocation(Tile tile)
    {
        commands.Add(gameInstance => CreateSpawnLocation.Command(
            gameInstance, gameInstance.Ids.GetNext<SpawnLocation>(), tile));
    }

    public void PlaceBuilding(
        IComponentOwnerBlueprint blueprint, PositionedFootprint footprint, ExternalId<Faction> externalId)
    {
        commands.Add(gameInstance => PlopBuilding.Command(
            gameInstance.State,
            gameInstance.State.Factions.Find(externalId),
            gameInstance.Ids.GetNext<GameObject>(),
            blueprint,
            footprint));
    }

    public void PlaceGameObject(IComponentOwnerBlueprint blueprint, Position3 position, Direction2 direction)
    {
        commands.Add(gameInstance => PlopComponentGameObject.Command(gameInstance, blueprint, position, direction));
    }

    public ImmutableArray<CommandFactory> ToCommandFactories()
    {
        return commands.ToImmutableArray();
    }
}