using System;
using System.Collections.Generic;
using System.Reflection;
using Bearded.TD.Content.Behaviors;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Content.Serialization.Models;

static class ComponentFactories
{
    private static readonly MethodInfo makeFactoryFactoryMethodInfo = typeof(ComponentFactories)
        .GetMethod(nameof(makeFactoryFactoryGeneric), BindingFlags.NonPublic | BindingFlags.Static);

    private static readonly BehaviorFactories<IComponent, ComponentAttribute, ComponentOwnerAttribute, VoidParameters> factories =
        new(typeof(IComponent<>), makeFactoryFactoryMethodInfo);

    public static void Initialize() => factories.Initialize();

    public static IDictionary<string, Type> ParameterTypesForComponentsById => factories.ParameterTypesById;

    public static IComponentFactory<TOwner> CreateComponentFactory<TOwner>(IComponent parameters) =>
        factories.CreateBehaviorFactory<TOwner>(parameters) as IComponentFactory<TOwner>;

    private static object makeFactoryFactoryGeneric<TOwner, TParameters>(
        Func<TParameters, IComponent<TOwner>> constructor)
        where TParameters : IParametersTemplate<TParameters>
    {
        return (Func<TParameters, object>) (p =>
            new ComponentFactory<TOwner, TParameters>(p, constructor));
    }
}