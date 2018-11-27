using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Navigation
{
    sealed class PassabilityLayer
    {
        private readonly Level level;
        private readonly Tilemap<TilePassability> tilemap;

        public PassabilityLayer(Level level)
        {
            this.level = level;
            tilemap = new Tilemap<TilePassability>(level.Radius);
        }

        // TODO: take buildings into account
        
        public bool HandleTilePassabilityChanged(Tile tile, bool isPassable)
        {
            var currentPassability = tilemap[tile];
            if (currentPassability.IsPassable == isPassable)
            {
                return false;
            }

            tilemap[tile] = currentPassability.WithPassability(isPassable);

            foreach (var dir in level.ValidDirectionsFrom(tile))
            {
                var neighbour = tile.Neighbour(dir);
                if (isPassable)
                    openDirection(neighbour, dir.Opposite());
                else
                    closeDirection(neighbour, dir.Opposite());
            }

            return true;
        }

        private void closeDirection(Tile tile, Direction direction)
        {
            var passability = tilemap[tile];
            var passableDirections = passability.PassableDirections.Except(direction);
            tilemap[tile] = passability.WithPassableDirections(passableDirections);
        }

        private void openDirection(Tile tile, Direction direction)
        {
            var passability = tilemap[tile];
            var passableDirections = passability.PassableDirections.And(direction);
            tilemap[tile] = passability.WithPassableDirections(passableDirections);
        }

        public TilePassability this[Tile tile] => tilemap[tile];
    }
}
