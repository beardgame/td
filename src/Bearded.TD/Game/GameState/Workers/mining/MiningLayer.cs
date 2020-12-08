using System.Collections.Generic;
using Bearded.TD.Game.GameState.Events;
using Bearded.TD.Game.GameState.World;
using Bearded.TD.Tiles;
using Bearded.Utilities.IO;

namespace Bearded.TD.Game.Workers
{
    sealed class MiningLayer : IListener<TileMined>
    {
        private readonly Logger logger;
        private readonly Level level;
        private readonly GeometryLayer geometryLayer;
        private readonly HashSet<Tile> tilesQueuedForMining = new HashSet<Tile>();

        public MiningLayer(Logger logger, GlobalGameEvents events, Level level, GeometryLayer geometryLayer)
        {
            this.logger = logger;
            this.level = level;
            this.geometryLayer = geometryLayer;

            events.Subscribe(this);
        }

        public bool CanTileBeMined(Tile tile) =>
            level.IsValid(tile)
            && geometryLayer[tile].Type == TileType.Wall
            && !IsTileQueuedForMining(tile);

        public bool IsTileQueuedForMining(Tile tile) => tilesQueuedForMining.Contains(tile);

        public void MarkTileForMining(Tile tile)
        {
            tilesQueuedForMining.Add(tile);
        }

        public void CancelTileForMining(Tile tile)
        {
            tilesQueuedForMining.Remove(tile);
        }

        public void HandleEvent(TileMined @event)
        {
            if (!tilesQueuedForMining.Remove(@event.Tile))
            {
                logger.Debug.Log($"Tile {@event.Tile} was mined without being queued.");
            }
        }
    }
}
