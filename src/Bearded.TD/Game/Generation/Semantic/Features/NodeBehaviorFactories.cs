using System;
using System.Collections.Generic;
using System.Reflection;
using Bearded.TD.Content.Behaviors;
using Template = Bearded.TD.Content.Serialization.Models.INodeBehavior;

namespace Bearded.TD.Game.Generation.Semantic.Features;

static class NodeBehaviorFactories
{
    private static readonly MethodInfo makeFactoryFactoryMethodInfo = typeof(NodeBehaviorFactories)
        .GetMethod(nameof(makeFactoryFactoryGeneric), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly
        BehaviorFactories<Template, NodeBehaviorAttribute, VoidParameters>
        factories =
            new(typeof(INodeBehavior), makeFactoryFactoryMethodInfo);

    public static void Initialize() => factories.Initialize();

    public static IDictionary<string, Type> ParameterTypesForComponentsById => factories.ParameterTypesById;

    public static INodeBehaviorFactory CreateNodeBehaviorFactory(Template template) =>
        (factories.CreateBehaviorFactory(template) as INodeBehaviorFactory)!;

    private static object makeFactoryFactoryGeneric<TParameters>(
        Func<TParameters, INodeBehavior> constructor)
    {
        return (Func<TParameters, object>) (p => new NodeBehaviorFactory<TParameters>(p, constructor));
    }
}
