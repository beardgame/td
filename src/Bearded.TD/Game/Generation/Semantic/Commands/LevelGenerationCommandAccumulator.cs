using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.World;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Generation.Semantic.Commands.LevelGenerationCommand;

namespace Bearded.TD.Game.Generation.Semantic.Commands
{
    sealed class LevelGenerationCommandAccumulator
    {
        private readonly List<LevelGenerationCommand> commands = new();

        public void PlaceBuilding(IBuildingBlueprint blueprint, PositionedFootprint footprint)
        {
            commands.Add(new PlaceBuilding(blueprint, footprint));
        }

        public ImmutableArray<CommandFactory> ToCommandFactories()
        {
            return commands.Select(LevelGenerationCommandConverter.ToCommandFactory).ToImmutableArray();
        }

        public void PlaceGameObject(IComponentOwnerBlueprint blueprint, Position3 position, Direction2 direction)
        {
            commands.Add(new PlaceGameObject(blueprint, position, direction));
        }
    }
}
