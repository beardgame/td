using System;
using System.Collections.Generic;
using System.Reflection;
using Bearded.TD.Content.Behaviors;
using Template = Bearded.TD.Content.Serialization.Models.INodeBehavior;

namespace Bearded.TD.Game.Generation.Semantic.Features
{
    static class NodeBehaviorFactories
    {
        private static readonly MethodInfo makeFactoryFactoryMethodInfo = typeof(NodeBehaviorFactories)
            .GetMethod(nameof(makeFactoryFactoryGeneric), BindingFlags.NonPublic | BindingFlags.Static)!;

        private static readonly
            BehaviorFactories<Template, NodeBehaviorAttribute, NodeBehaviorOwnerAttribute, VoidParameters>
            factories =
                new(typeof(INodeBehavior<>), makeFactoryFactoryMethodInfo);

        public static void Initialize() => factories.Initialize();

        public static IDictionary<string, Type> ParameterTypesForComponentsById => factories.ParameterTypesById;

        public static INodeBehaviorFactory<Node> CreateNodeBehaviorFactory(Template template) =>
            CreateNodeBehaviorFactory<Node>(template);

        public static INodeBehaviorFactory<TOwner> CreateNodeBehaviorFactory<TOwner>(Template template) =>
            (factories.CreateBehaviorFactory<Node>(template) as INodeBehaviorFactory<TOwner>)!;

        // ReSharper disable once UnusedTypeParameter
        private static object makeFactoryFactoryGeneric<TOwner, TParameters>(
            Func<TParameters, INodeBehavior<TOwner>> constructor)
        {
            return (Func<TParameters, object>) (p => new NodeBehaviorFactory<TOwner, TParameters>(p, constructor));
        }
    }
}
