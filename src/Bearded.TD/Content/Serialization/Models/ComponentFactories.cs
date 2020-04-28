using System;
using System.Collections.Generic;
using System.Reflection;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Components;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Content.Serialization.Models
{
    static class ComponentFactories
    {
        private static readonly BehaviorFactories<IComponent, ComponentAttribute, ComponentOwnerAttribute> factories =
            new BehaviorFactories<IComponent, ComponentAttribute, ComponentOwnerAttribute>(typeof(IComponent<>));

        public static void Initialize() => factories.Initialize();

        public static IDictionary<string, Type> ParameterTypesForComponentsById => factories.ParameterTypesById;

        public static IComponentFactory<TOwner> CreateComponentFactory<TOwner>(IComponent parameters)
            => wrapFactory(factories.CreateBehaviorFactory<TOwner>(parameters));

        public static BuildingComponentFactory CreateBuildingComponentFactory(IBuildingComponent parameters)
        {
            var forBuilding = factories.CreateBehaviorFactory<Building>(parameters);
            var forGhost = factories.CreateBehaviorFactory<BuildingGhost>(parameters);
            var forPlaceholder = factories.CreateBehaviorFactory<BuildingPlaceholder>(parameters);

            return new BuildingComponentFactory(
                parameters, wrapFactory(forBuilding), wrapFactory(forGhost), wrapFactory(forPlaceholder));
        }

        public static void Initialise()
        {
            factories.Initialize();
        }

        private static IComponentFactory<TOwner> wrapFactory<TOwner>(IBehaviorFactory<TOwner> behaviorFactory)
        {
            if (behaviorFactory == null) return null;

            var args = behaviorFactory.GetType().GetGenericArguments();
            var methodInfo = typeof(ComponentFactories)
                .GetMethod(nameof(wrapFactoryGeneric), BindingFlags.Static | BindingFlags.NonPublic);
            var genericMethod = methodInfo.MakeGenericMethod(args);

            return (IComponentFactory<TOwner>) genericMethod.Invoke(null, new object[] {behaviorFactory});
        }

        private static IComponentFactory<TOwner> wrapFactoryGeneric<TOwner, TParameters>(
            IBehaviorFactory<TOwner> behaviorFactory) where TParameters : IParametersTemplate<TParameters>
        {
            var concreteFactory = (BehaviorFactory<TOwner, TParameters>) behaviorFactory;
            return new ComponentFactory<TOwner, TParameters>(concreteFactory.UglyWayToAccessParameters, _ => concreteFactory.Create());
        }
    }
}
