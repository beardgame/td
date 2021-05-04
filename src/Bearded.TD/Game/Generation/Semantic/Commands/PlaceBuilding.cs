using Bearded.TD.Game.Commands.Loading;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.World;

namespace Bearded.TD.Game.Generation.Semantic.Commands
{
    sealed record PlaceBuilding(IBuildingBlueprint Blueprint, PositionedFootprint Footprint) : ILevelGenerationCommand
    {
        public CommandFactory ToCommandFactory()
        {
            return gameInstance => PlopBuilding.Command(
                gameInstance.State,
                gameInstance.State.RootFaction,
                gameInstance.Ids.GetNext<Building>(),
                Blueprint,
                Footprint);
        }
    }
}
