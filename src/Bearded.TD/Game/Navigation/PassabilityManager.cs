using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Events;
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Navigation
{
    sealed class PassabilityManager
        : IListener<TileTypeChanged>, IListener<BuildingConstructionStarted>, IListener<BuildingDestroyed>
    {
        private static readonly Dictionary<TileType, Passabilities>
            passabilityByTileType = new Dictionary<TileType, Passabilities>
            {
                { TileType.Unknown, Passabilities.All },
                { TileType.Floor, Passabilities.All },
                { TileType.Wall, Passabilities.None },
                { TileType.Crevice, ~Passabilities.WalkingUnit }
            };
        private static readonly Passabilities passabilityInBuilding = ~Passabilities.WalkingUnit;

        private readonly GlobalGameEvents events;
        private readonly GeometryLayer geometryLayer;
        private readonly BuildingLayer buildingLayer;

        private readonly Dictionary<Passability, PassabilityLayer> passabilityLayers;

        public PassabilityManager(
            GlobalGameEvents events, Level level, GeometryLayer geometryLayer, BuildingLayer buildingLayer)
        {
            this.events = events;
            this.geometryLayer = geometryLayer;
            this.buildingLayer = buildingLayer;

            events.Subscribe<TileTypeChanged>(this);
            events.Subscribe<BuildingConstructionStarted>(this);
            events.Subscribe<BuildingDestroyed>(this);

            passabilityLayers = ((Passability[]) Enum.GetValues(typeof(Passability)))
                .ToDictionary(
                    p => p,
                    p => new PassabilityLayer(level, extraConditionsFor(p))
                    );
        }

        private IEnumerable<Func<Tile, Tile, bool>> extraConditionsFor(Passability p)
        {
            switch (p)
            {
                case Passability.WalkingUnit:
                    yield return tileHeightDifferenceIsWalkable;
                    break;
            }
        }

        private bool tileHeightDifferenceIsWalkable(Tile t0, Tile t1)
        {
            var h0 = geometryLayer[t0].Geometry.FloorHeight;
            var h1 = geometryLayer[t1].Geometry.FloorHeight;
            var heightDifference = Math.Abs((h0 - h1).NumericValue).U();

            return heightDifference < Constants.Game.Navigation.MaxWalkableHeightDifference;
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
