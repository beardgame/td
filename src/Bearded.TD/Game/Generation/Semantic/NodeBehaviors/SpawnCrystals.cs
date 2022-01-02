using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors;

// TODO: this should probably not just spawn crystals, but right now the locations it picks out are very tightly
//       coupled to the positioning of crystals.
[NodeBehavior("spawnCrystals")]
sealed class SpawnCrystals : NodeBehavior<SpawnCrystals.BehaviorParameters>
{
    public SpawnCrystals(BehaviorParameters parameters) : base(parameters) { }

    public override void Generate(NodeGenerationContext context)
    {
        foreach (var tile in context.Tiles.All)
        {
            var centerGeometry = context.Tiles.Get(tile);

            if (centerGeometry.Type == TileType.Wall)
            {
                continue;
            }

            var height = centerGeometry.FloorHeight;

            if (centerGeometry.Type == TileType.Floor && context.Random.NextBool(0.1))
            {
                var position = Level.GetPosition(tile)
                    + Direction2.FromDegrees(context.Random.NextFloat(360))
                    // TODO: this radius number is somewhat of a guess right now to ensure it is always within the
                    //       tile, even with the offset below
                    * context.Random.NextFloat(0, Constants.Game.World.HexagonDistanceX * 0.4f).U();
                var z = height + 0.01.U();
                var count = context.Random.Next(3, 7);
                foreach (var _ in Enumerable.Range(0, count))
                {
                    // TODO: this sometimes leads to crystals being placed outside of allowed tiles, uh oh!
                    var direction = Direction2.FromDegrees(context.Random.NextFloat(360));
                    var offset = direction * (context.Random.NextFloat(2f, 5f) * Constants.Rendering.PixelSize).U();

                    context.Content.PlaceGameObject(
                        Parameters.Blueprints.RandomElement(context.Random),
                        (position + offset).WithZ(z),
                        direction);
                }
            }

            foreach (var direction in Directions.All.Enumerate())
            {
                var neighbor = tile.Neighbor(direction);
                var neighborGeometry = context.Tiles.Get(neighbor);

                switch (centerGeometry.Type, neighborGeometry.Type)
                {
                    case (_, TileType.Floor):
                    case (TileType.Floor, TileType.Wall):
                        break;
                    default:
                        continue;
                }

                var neighborHeight = neighborGeometry.FloorHeight;

                if (height + Constants.Game.Navigation.MaxWalkableHeightDifference > neighborHeight
                    || context.Random.NextBool(0.90))
                {
                    continue;
                }

                var dirVector = direction.Vector();
                var dir = -direction.SpaceTimeDirection();

                var count = centerGeometry.Type == TileType.Crevice
                    ? context.Random.Next(3, 12)
                    : context.Random.Next(2, 4);

                var zFactorCenter = centerGeometry.Type == TileType.Crevice
                    ? context.Random.NextFloat(0.6f, 0.8f)
                    : context.Random.NextFloat(0.025f, 0.3f);

                foreach (var _ in Enumerable.Range(0, count))
                {
                    var offset = context.Random.NextFloat(-1, 1);
                    var offsetAngle = (offset * 30).Degrees();
                    var offsetPosition = offset * dirVector.PerpendicularRight
                        * Constants.Game.World.HexagonSide * 0.3.U();

                    var zFactor = zFactorCenter * context.Random.NextFloat(0.8f, 1.2f);

                    var position = Level.GetPosition(tile) + dirVector
                        * (Constants.Game.World.HexagonWidth * 0.5.U() - Constants.Rendering.PixelSize.U() * 1.5f);

                    var z = height + zFactor * (neighborHeight - height);

                    context.Content.PlaceGameObject(
                        Parameters.Blueprints.RandomElement(context.Random),
                        (position + offsetPosition).WithZ(z),
                        dir + offsetAngle);
                }
            }
        }
    }

    public sealed record BehaviorParameters(ImmutableArray<IComponentOwnerBlueprint> Blueprints);
}