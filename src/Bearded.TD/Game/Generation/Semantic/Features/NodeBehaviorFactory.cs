using System;

namespace Bearded.TD.Game.Generation.Semantic.Features
{
    interface INodeBehaviorFactory
    {
        INodeBehavior Create();
    }

    sealed class NodeBehaviorFactory<TParameters> : INodeBehaviorFactory
    {
        private readonly TParameters parameters;
        private readonly Func<TParameters, INodeBehavior> factory;

        public NodeBehaviorFactory(TParameters parameters, Func<TParameters, INodeBehavior> factory)
        {
            this.parameters = parameters;
            this.factory = factory;
        }

        public INodeBehavior Create() => factory(parameters);
    }
}
