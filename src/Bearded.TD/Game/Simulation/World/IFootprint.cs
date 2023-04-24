using System.Collections.Generic;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.World;

interface IFootprint : IBlueprint
{
    public IEnumerable<Tile> OccupiedTiles(Tile rootTile);
    public Position2 Center(Tile rootTile);
    public Tile RootTileClosestToWorldPosition(Position2 position);
}
