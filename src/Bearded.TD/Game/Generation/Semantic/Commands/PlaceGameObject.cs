using Bearded.TD.Game.Commands.Loading;
using Bearded.TD.Game.Simulation.Components;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation.Semantic.Commands
{
    sealed record PlaceGameObject(
        IComponentOwnerBlueprint Blueprint,
        Position3 Position,
        Direction2 Direction) : ILevelGenerationCommand
    {
        public CommandFactory ToCommandFactory()
        {
            return gameInstance => PlopComponentGameObject.Command(gameInstance, Blueprint, Position, Direction);
        }
    }
}
