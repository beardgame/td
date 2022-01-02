using System;

namespace Bearded.TD.Game.Generation.Semantic.Features;

interface INodeBehaviorFactory<TOwner>
{
    INodeBehavior<TOwner> Create();
}

sealed class NodeBehaviorFactory<TOwner, TParameters> : INodeBehaviorFactory<TOwner>
{
    private readonly TParameters parameters;
    private readonly Func<TParameters, INodeBehavior<TOwner>> factory;

    public NodeBehaviorFactory(TParameters parameters, Func<TParameters, INodeBehavior<TOwner>> factory)
    {
        this.parameters = parameters;
        this.factory = factory;
    }

    public INodeBehavior<TOwner> Create() => factory(parameters);
}