using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.Features
{
    interface INodeBehavior
    {
        string Name => Regex.Replace(GetType().Name, "(Node)?(Behaviou?r)?$", "");
        ImmutableArray<NodeTag> Tags => ImmutableArray<NodeTag>.Empty;

        double GetFitnessPenalty(INodeFitnessContext context, Tile nodeTile) => 0;
        void Generate(NodeGenerationContext context) {}
    }
}
