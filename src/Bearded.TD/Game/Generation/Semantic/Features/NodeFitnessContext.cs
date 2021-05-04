using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.Features
{
    interface INodeFitnessContext
    {
        public PlacedNode this[Tile tile] { get; }
    }
}
