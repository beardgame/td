using System;
using System.Collections.Generic;
using System.Reflection;
using Bearded.TD.Content.Behaviors;
using Bearded.TD.Game.Generation.Semantic.Features;
using ISimulationNodeBehavior = Bearded.TD.Game.Generation.Semantic.Features.INodeBehavior;

namespace Bearded.TD.Content.Serialization.Models;

static class NodeBehaviorFactories
{
    private static readonly MethodInfo makeFactoryFactoryMethodInfo = typeof(NodeBehaviorFactories)
        .GetMethod(nameof(makeFactoryFactoryGeneric), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly
        BehaviorFactories<INodeBehavior, NodeBehaviorAttribute, VoidParameters>
        factories =
            new(typeof(ISimulationNodeBehavior), makeFactoryFactoryMethodInfo);

    public static void Initialize() => factories.Initialize();

    public static IDictionary<string, Type> ParameterTypesForComponentsById => factories.ParameterTypesById;

    public static INodeBehaviorFactory CreateNodeBehaviorFactory(INodeBehavior template) =>
        (factories.CreateBehaviorFactory(template) as INodeBehaviorFactory)!;

    private static object makeFactoryFactoryGeneric<TParameters>(
        Func<TParameters, INodeBehavior, ISimulationNodeBehavior> constructor)
    {
        return (Func<TParameters, INodeBehavior, object>) ((p, m) => new NodeBehaviorFactory<TParameters>(p, m, constructor));
    }

    private sealed class NodeBehaviorFactory<TParameters> : INodeBehaviorFactory
    {
        private readonly TParameters parameters;
        private readonly INodeBehavior model;
        private readonly Func<TParameters, INodeBehavior, ISimulationNodeBehavior> factory;

        public NodeBehaviorFactory(
            TParameters parameters,
            INodeBehavior model,
            Func<TParameters, INodeBehavior, ISimulationNodeBehavior> factory)
        {
            this.parameters = parameters;
            this.model = model;
            this.factory = factory;
        }

        public ISimulationNodeBehavior Create() => factory(parameters, model);
    }
}
