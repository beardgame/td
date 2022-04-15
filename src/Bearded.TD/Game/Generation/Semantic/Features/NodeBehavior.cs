using System.Collections.Immutable;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.Features;

abstract class NodeBehavior<TParameters> : INodeBehavior
{
    protected TParameters Parameters { get; }

    public virtual ImmutableArray<NodeTag> Tags => ImmutableArray<NodeTag>.Empty;

    protected NodeBehavior(TParameters parameters)
    {
        Parameters = parameters;
    }

    public virtual double GetFitnessPenalty(INodeFitnessContext context, Tile nodeTile) => 0;

    public virtual void Generate(NodeGenerationContext context) {}
}

abstract class NodeBehavior : NodeBehavior<VoidParameters>
{
    protected NodeBehavior() : base(new VoidParameters())
    {
    }
}
