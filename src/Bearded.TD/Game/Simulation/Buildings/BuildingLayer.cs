using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
        private readonly Dictionary<Tile, IBuilding> buildingLookup = new();

        public BuildingLayer(GlobalGameEvents events)
        {
            this.events = events;
        }

        public void AddBuilding(IBuilding building)
        {
            foreach (var tile in OccupiedTileAccumulator.AccumulateOccupiedTiles(building))
            {
                DebugAssert.State.Satisfies(!buildingLookup.ContainsKey(tile));
                buildingLookup.Add(tile, building);
            }
        }

        public void RemoveBuilding(IBuilding building)
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

        private IBuildingState? getBuildingStateFor(Tile tile)
        {
            return GetBuildingFor(tile)?.GetComponents<IBuildingStateProvider>().SingleOrDefault()?.State;
        }

        public IBuilding? GetBuildingFor(Tile tile)
        {
            buildingLookup.TryGetValue(tile, out var building);
            return building;
        }

        public bool TryGetMaterializedBuilding(Tile tile, [NotNullWhen(true)] out IBuilding? building)
        {
            building = GetBuildingFor(tile);
            return getBuildingStateFor(tile)?.IsMaterialized ?? false;
        }

        public Occupation GetOccupationFor(Tile tile)
        {
            var state = getBuildingStateFor(tile);
            return state switch
            {
                null => Occupation.None,
                {IsMaterialized: true} => Occupation.MaterializedBuilding,
                _ => Occupation.ReservedForBuilding
            };
        }

        public IBuilding? this[Tile tile] => GetBuildingFor(tile);
    }
}
