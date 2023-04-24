using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.World;

sealed class Footprint : IFootprint
{
    public static readonly Footprint Single = FromOffsets(new[] { new Step(0, 0) });

    public static Footprint FromOffsets(IEnumerable<Step> tileOffsets) => new(ModAwareId.Invalid, tileOffsets);

    private readonly ImmutableArray<Step> tileOffsets;
    private readonly Difference2 rootTileOffset; // vector that points from center of footprint to center of root tile

    public ModAwareId Id { get; }

    public Footprint(ModAwareId id, IEnumerable<Step> tileOffsets)
    {
        Id = id;
        this.tileOffsets = tileOffsets.ToImmutableArray();
        rootTileOffset = -this.tileOffsets
            .Select(step =>
                step.X * Constants.Game.World.HexagonGridUnitX + step.Y * Constants.Game.World.HexagonGridUnitY)
            .Aggregate((diff1, diff2) => diff1 + diff2) / this.tileOffsets.Length;
    }

    public IEnumerable<Tile> OccupiedTiles(Tile rootTile) => tileOffsets.Select(offset => rootTile + offset);

    public Position2 Center(Tile rootTile) => Level.GetPosition(rootTile) - rootTileOffset;

    public Tile RootTileClosestToWorldPosition(Position2 position) => Level.GetTile(position + rootTileOffset);
}
