using System;
using System.Collections.Generic;
using System.Reflection;
using Bearded.TD.Content.Behaviors;
using Bearded.TD.Game.Simulation.Components;
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
        Func<TParameters, ISimulationComponent> constructor)
        where TParameters : IParametersTemplate<TParameters>
    {
        return (Func<TParameters, object>) (p => new ComponentFactory<TParameters>(p, constructor));
    }
}
