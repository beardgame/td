using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using static Bearded.TD.Game.Simulation.Buildings.IBuildBuildingPrecondition;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Footprints;

interface IFootprintGroup
{
    World.FootprintGroup FootprintGroup { get; }
}

[Component("footprint")]
sealed class FootprintGroup : Component<Content.Models.IFootprintGroup>, IFootprintGroup, IBuildBuildingPrecondition
{
    World.FootprintGroup IFootprintGroup.FootprintGroup => Parameters.Group;

    public FootprintGroup(Content.Models.IFootprintGroup parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public Result CanBuild(Parameters parameters)
    {
        if (Parameters.Group != parameters.Footprint.Footprint)
            return Result.InValid;

        var invalidTiles = parameters.Footprint.OccupiedTiles
            .Where(t => !isTileValidForBuilding(parameters.Game, t))
            .ToImmutableArray();
        var invalidEdges = getUnwalkableEdges(parameters.Game, parameters.Footprint.OccupiedTiles)
            .ToImmutableArray();

        var canBuild = invalidTiles.Length == 0 && invalidEdges.Length == 0;

        return canBuild
            ? Result.Valid
            : Result.InValid
                with
                {
                    BadTiles = invalidTiles,
                    BadEdges = invalidEdges
                };
    }

    private static bool isTileValidForBuilding(GameState game, Tile tile)
    {
        return game.Level.IsValid(tile)
            && game.GeometryLayer[tile].Type == TileType.Floor
            && game.BuildingLayer.GetOccupationFor(tile) == BuildingLayer.Occupation.None
            && game.VisibilityLayer[tile].IsRevealed();
    }

    private static IEnumerable<TileEdge> getUnwalkableEdges(GameState game, IEnumerable<Tile> tiles)
    {
        var passability = game.PassabilityManager.GetLayer(Passability.WalkingUnit);
        var tilesSet = new HashSet<Tile>(tiles);

        foreach (var tile in tilesSet)
        {
            foreach (var direction in Directions.All.Enumerate())
            {
                if (!tilesSet.Contains(tile.Neighbor(direction)))
                    continue;
                if (isEdgeWalkable(passability, tile, direction))
                    continue;
                yield return TileEdge.From(tile, direction);
            }
        }
    }

    private static bool isEdgeWalkable(PassabilityLayer passability, Tile t0, Direction direction)
    {
        var t1 = t0.Neighbor(direction);

        return passability[t0].PassableDirections.Includes(direction)
            && passability[t1].PassableDirections.Includes(direction.Opposite());
    }
}
