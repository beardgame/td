using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.Features
{
    // ReSharper disable once UnusedTypeParameter
    interface INodeBehavior<TOwner>
    {
        string Name => Regex.Replace(GetType().Name, "(Node)?(Behaviou?r)?$", "");
        ImmutableArray<NodeTag> Tags { get; }

        double GetFitnessPenalty(INodeFitnessContext context, Tile nodeTile);
        void Generate(NodeGenerationContext context);
    }
}
