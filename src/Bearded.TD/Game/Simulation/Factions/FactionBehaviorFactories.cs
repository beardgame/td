using System;
using System.Collections.Generic;
using System.Reflection;
using Bearded.TD.Content.Behaviors;
using Template = Bearded.TD.Content.Serialization.Models.IFactionBehavior;

namespace Bearded.TD.Game.Simulation.Factions;

static class FactionBehaviorFactories
{
    private static readonly MethodInfo makeFactoryFactoryMethodInfo = typeof(FactionBehaviorFactories)
        .GetMethod(nameof(makeFactoryFactoryGeneric), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly
        BehaviorFactories<Template, FactionBehaviorAttribute, VoidParameters>
        factories =
            new(typeof(IFactionBehavior), makeFactoryFactoryMethodInfo);

    public static void Initialize() => factories.Initialize();

    public static IDictionary<string, Type> ParameterTypesForComponentsById => factories.ParameterTypesById;

    public static IFactionBehaviorFactory CreateFactionBehaviorFactory(Template template) =>
        (factories.CreateBehaviorFactory(template) as IFactionBehaviorFactory)!;

    private static object makeFactoryFactoryGeneric<TParameters>(
        Func<TParameters, IFactionBehavior> constructor)
    {
        return (Func<TParameters, object>) (p => new FactionBehaviorFactory<TParameters>(p, constructor));
    }
}
