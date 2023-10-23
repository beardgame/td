using System;
using System.Collections.Generic;
using System.Reflection;
using Bearded.TD.Content.Behaviors;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using ISimulationComponent = Bearded.TD.Game.Simulation.GameObjects.IComponent;

namespace Bearded.TD.Content.Serialization.Models;

static class ComponentFactories
{
    private static readonly MethodInfo makeFactoryFactoryMethodInfo = typeof(ComponentFactories)
        .GetMethod(nameof(makeFactoryFactoryGeneric), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly BehaviorFactories<IComponent, ComponentAttribute, VoidParameters> factories =
        new(typeof(ISimulationComponent), makeFactoryFactoryMethodInfo);

    public static void Initialize() => factories.Initialize();

    public static IDictionary<string, Type> ParameterTypesForComponentsById => factories.ParameterTypesById;

    public static IComponentFactory CreateComponentFactory(IComponent parameters) =>
        (factories.CreateBehaviorFactory(parameters) as IComponentFactory)!;

    private static object makeFactoryFactoryGeneric<TParameters>(
        Func<TParameters, IComponent, ISimulationComponent> constructor)
        where TParameters : IParametersTemplate<TParameters>
    {
        return (Func<TParameters, IComponent, object>)
            ((p, model) => new ComponentFactory<TParameters>(p, model, constructor));
    }

    private sealed class ComponentFactory<TComponentParameters> : IComponentFactory
        where TComponentParameters : IParametersTemplate<TComponentParameters>
    {
        private readonly TComponentParameters parameters;
        private readonly IComponent model;
        private readonly Func<TComponentParameters, IComponent, ISimulationComponent> factory;

        public ComponentFactory(
            TComponentParameters parameters,
            IComponent model,
            Func<TComponentParameters, IComponent, ISimulationComponent> factory)
        {
            this.parameters = parameters;
            this.model = model;
            this.factory = factory;
        }

        public ISimulationComponent Create() => factory(parameters, model);
    }
}
