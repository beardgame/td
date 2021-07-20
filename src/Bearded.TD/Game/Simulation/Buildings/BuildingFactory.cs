using Bearded.TD.Game.Simulation.Components.Generic;
using Bearded.TD.Game.Simulation.Components.Statistics;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.World;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Buildings
{
    sealed class BuildingFactory
    {
        private readonly GameState gameState;

        public BuildingFactory(GameState gameState)
        {
            this.gameState = gameState;
        }

        // TODO: should return a ComponentGameObject instead
        public Building Create(
            Id<Building> id, IBuildingBlueprint blueprint, Faction faction, PositionedFootprint footprint)
        {
            var building = new Building(id, blueprint, faction);
            gameState.Add(building);
            building.AddComponent(new StaticTileOccupation<Building>(footprint));
            building.AddComponent(new StatisticCollector<Building>());
            building.AddComponent(new Selectable<Building>());
            gameState.BuildingLayer.AddBuilding(building);
            return building;
        }
    }
}
