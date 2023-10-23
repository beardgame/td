using System;
using System.Collections.Generic;
using System.Reflection;
using Bearded.TD.Content.Behaviors;
using Bearded.TD.Game.Simulation.Factions;
using ISimulationFactionBehavior = Bearded.TD.Game.Simulation.Factions.IFactionBehavior;

namespace Bearded.TD.Content.Serialization.Models;

static class FactionBehaviorFactories
{
    private static readonly MethodInfo makeFactoryFactoryMethodInfo = typeof(FactionBehaviorFactories)
        .GetMethod(nameof(makeFactoryFactoryGeneric), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly
        BehaviorFactories<IFactionBehavior, FactionBehaviorAttribute, VoidParameters>
        factories =
            new(typeof(ISimulationFactionBehavior), makeFactoryFactoryMethodInfo);

    public static void Initialize() => factories.Initialize();

    public static IDictionary<string, Type> ParameterTypesForComponentsById => factories.ParameterTypesById;

    public static IFactionBehaviorFactory CreateFactionBehaviorFactory(IFactionBehavior template) =>
        (factories.CreateBehaviorFactory(template) as IFactionBehaviorFactory)!;

    private static object makeFactoryFactoryGeneric<TParameters>(
        Func<TParameters, IFactionBehavior, ISimulationFactionBehavior> constructor)
    {
        return (Func<TParameters, IFactionBehavior, object>) ((p, m) => new FactionBehaviorFactory<TParameters>(p, m, constructor));
    }

    private sealed class FactionBehaviorFactory<TParameters> : IFactionBehaviorFactory
    {
        private readonly TParameters parameters;
        private readonly IFactionBehavior model;
        private readonly Func<TParameters, IFactionBehavior, ISimulationFactionBehavior> factory;

        public FactionBehaviorFactory(
            TParameters parameters,
            IFactionBehavior model,
            Func<TParameters, IFactionBehavior, ISimulationFactionBehavior> factory)
        {
            this.parameters = parameters;
            this.model = model;
            this.factory = factory;
        }

        public Game.Simulation.Factions.IFactionBehavior Create() => factory(parameters, model);
    }
}
