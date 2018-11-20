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

        private readonly Dictionary<Tile, IPlacedBuilding> buildingLookup = new Dictionary<Tile, IPlacedBuilding>();

        public void AddBuilding(IPlacedBuilding building)
        {
            foreach (var tile in building.OccupiedTiles)
            {
                DebugAssert.State.Satisfies(!buildingLookup.ContainsKey(tile));
                buildingLookup.Add(tile, building);
            }
        }

        public void RemoveBuilding(IPlacedBuilding building)
        {
            foreach (var tile in building.OccupiedTiles)
            {
                DebugAssert.State.Satisfies(buildingLookup.ContainsKey(tile) && buildingLookup[tile] == building);
                buildingLookup.Remove(tile);
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
