using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.World;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation.Semantic.Commands
{
    abstract record LevelGenerationCommand
    {
        private LevelGenerationCommand() {}

        public sealed record PlaceBuilding(IBuildingBlueprint Blueprint, PositionedFootprint Footprint)
            : LevelGenerationCommand;

        public sealed record PlaceGameObject(
            IComponentOwnerBlueprint Blueprint,
            Position3 Position,
            Direction2 Direction) : LevelGenerationCommand;
    }
}
