using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Simulation.Buildings
{
    sealed class BuildingLayer
    {
        public enum Occupation
        {
            None,
            ReservedForBuilding, // a building is scheduled to be built in this tile, but hasn't yet
            MaterializedBuilding
        }

        private readonly GlobalGameEvents events;
        private readonly Dictionary<Tile, IPlacedBuilding> buildingLookup = new();

        public BuildingLayer(GlobalGameEvents events)
        {
            this.events = events;
        }

        public void AddBuilding(IPlacedBuilding building)
        {
            foreach (var tile in OccupiedTileAccumulator.AccumulateOccupiedTiles(building))
            {
                DebugAssert.State.Satisfies(!buildingLookup.ContainsKey(tile));
                buildingLookup.Add(tile, building);
            }
        }

        public void RemoveBuilding(IPlacedBuilding building)
        {
            foreach (var tile in OccupiedTileAccumulator.AccumulateOccupiedTiles(building))
            {
                DebugAssert.State.Satisfies(buildingLookup.ContainsKey(tile) && buildingLookup[tile] == building);
                buildingLookup.Remove(tile);
            }

            if (building is Building finishedBuilding)
            {
                events.Send(new BuildingDestroyed(finishedBuilding));
            }
        }

        public IPlacedBuilding? GetBuildingFor(Tile tile)
        {
            buildingLookup.TryGetValue(tile, out var building);
            return building;
        }

        public bool TryGetMaterializedBuilding(Tile tile, [NotNullWhen(true)] out IPlacedBuilding? building)
        {
            building = GetBuildingFor(tile);
            return building?.State.IsMaterialized ?? false;
        }

        public Occupation GetOccupationFor(Tile tile)
        {
            var building = GetBuildingFor(tile);
            return building switch
            {
                null => Occupation.None,
                {State: {IsMaterialized: true}} => Occupation.MaterializedBuilding,
                _ => Occupation.ReservedForBuilding
            };
        }

        public IPlacedBuilding? this[Tile tile] => GetBuildingFor(tile);
    }
}
