using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Events;
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.Navigation
{
    sealed class PassabilityManager
        : IListener<TileTypeChanged>, IListener<BuildingConstructionStarted>, IListener<BuildingDestroyed>
    {
        private static readonly Dictionary<TileGeometry.TileType, Passabilities>
            passabilityByTileType = new Dictionary<TileGeometry.TileType, Passabilities>
            {
                { TileGeometry.TileType.Unknown, Passabilities.All },
                { TileGeometry.TileType.Floor, Passabilities.All },
                { TileGeometry.TileType.Wall, Passabilities.None },
                { TileGeometry.TileType.Crevice, ~Passabilities.WalkingUnit }
            };
        private static readonly Passabilities passabilityInBuilding = ~Passabilities.WalkingUnit;

        private readonly GameEvents events;
        private readonly GeometryLayer geometryLayer;
        private readonly BuildingLayer buildingLayer;

        private readonly Dictionary<Passability, PassabilityLayer> passabilityLayers;

        public PassabilityManager(
            GameEvents events, Level level, GeometryLayer geometryLayer, BuildingLayer buildingLayer)
        {
            this.events = events;
            this.geometryLayer = geometryLayer;
            this.buildingLayer = buildingLayer;

            events.Subscribe<TileTypeChanged>(this);
            events.Subscribe<BuildingConstructionStarted>(this);
            events.Subscribe<BuildingDestroyed>(this);

            passabilityLayers = ((Passability[]) Enum.GetValues(typeof(Passability)))
                .ToDictionary(p => p, _ => new PassabilityLayer(level));
        }

        public void HandleEvent(TileTypeChanged @event) => updatePassabilityForTile(@event.Tile);

        public void HandleEvent(BuildingConstructionStarted @event)
            => @event.Building.OccupiedTiles.ForEach(updatePassabilityForTile);

        public void HandleEvent(BuildingDestroyed @event)
            => @event.Builder.OccupiedTiles.ForEach(updatePassabilityForTile);

        private void updatePassabilityForTile(Tile tile)
        {
            var type = geometryLayer[tile].Type;
            var hasBuilding = buildingLayer.GetOccupationFor(tile) == BuildingLayer.Occupation.FinishedBuilding;

            var hasChangedPassability = false;

            foreach (var (passability, layer) in passabilityLayers)
            {
                var passabilityFlags = passability.AsFlags();
                var isTypePassable = (passabilityFlags & passabilityByTileType[type]) != Passabilities.None;
                var isBuildingPassable
                    = !hasBuilding || (passabilityFlags & passabilityInBuilding) != Passabilities.None;
                hasChangedPassability |= layer.HandleTilePassabilityChanged(tile, isTypePassable && isBuildingPassable);
            }

            if (hasChangedPassability)
            {
                events.Send(new TilePassabilityChanged(tile));
            }
        }

        public PassabilityLayer GetLayer(Passability passability) => passabilityLayers[passability];
    }
}
