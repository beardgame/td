using System;
using System.Collections.Generic;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Buildings
{
    sealed class BuildingLayer
    {

        public enum Occupation
        {
            None,
            ReservedForBuilding, // a building is scheduled to be built in this tile, but hasn't yet
            FinishedBuilding
        }
        
        private readonly GameEvents events;
        private readonly Dictionary<Tile, IPlacedBuilding> buildingLookup = new Dictionary<Tile, IPlacedBuilding>();

        public BuildingLayer(GameEvents events)
        {
            this.events = events;
        }

        public void AddBuilding(IPlacedBuilding building)
        {
            foreach (var tile in building.OccupiedTiles)
            {
                DebugAssert.State.Satisfies(!buildingLookup.ContainsKey(tile));
                buildingLookup.Add(tile, building);
            }

            if (building is Building finishedBuilding)
            {
                events.Send(new BuildingCreated(finishedBuilding));
            }
        }

        public void RemoveBuilding(IPlacedBuilding building)
        {
            foreach (var tile in building.OccupiedTiles)
            {
                DebugAssert.State.Satisfies(buildingLookup.ContainsKey(tile) && buildingLookup[tile] == building);
                buildingLookup.Remove(tile);
            }

            if (building is Building finishedBuilding)
            {
                events.Send(new BuildingDestroyed(finishedBuilding));
            }
        }

        public IPlacedBuilding GetBuildingOnTile(Tile tile)
        {
            buildingLookup.TryGetValue(tile, out var building);
            return building;
        }

        public Occupation GetOccupationForTile(Tile tile)
        {
            var building = GetBuildingOnTile(tile);
            switch (building)
            {
                case null:
                    return Occupation.None;
                case Building _:
                    return Occupation.FinishedBuilding;
                case BuildingPlaceholder _:
                    return Occupation.ReservedForBuilding;
            }
            throw new InvalidOperationException();
        }
    }
}
