using System;
using System.Collections.Generic;
using System.Reflection;
using Bearded.TD.Content.Behaviors;
using Template = Bearded.TD.Content.Serialization.Models.IFactionBehavior;

namespace Bearded.TD.Game.Simulation.Factions
{
    static class FactionBehaviorFactories
    {
        private static readonly MethodInfo makeFactoryFactoryMethodInfo = typeof(FactionBehaviorFactories)
            .GetMethod(nameof(makeFactoryFactoryGeneric), BindingFlags.NonPublic | BindingFlags.Static)!;

        private static readonly
            BehaviorFactories<Template, FactionBehaviorAttribute, FactionBehaviorOwnerAttribute, VoidParameters>
            factories =
                new(typeof(IFactionBehavior<>), makeFactoryFactoryMethodInfo);

        public static void Initialize() => factories.Initialize();

        public static IDictionary<string, Type> ParameterTypesForComponentsById => factories.ParameterTypesById;

        public static IFactionBehaviorFactory<Faction> CreateFactionBehaviorFactory(Template template) =>
            CreateFactionBehaviorFactory<Faction>(template);

        public static IFactionBehaviorFactory<TOwner> CreateFactionBehaviorFactory<TOwner>(Template template) =>
            (factories.CreateBehaviorFactory<TOwner>(template) as IFactionBehaviorFactory<TOwner>)!;

        private static object makeFactoryFactoryGeneric<TOwner, TParameters>(
            Func<TParameters, IFactionBehavior<TOwner>> constructor)
        {
            return (Func<TParameters, object>) (p => new FactionBehaviorFactory<TOwner, TParameters>(p, constructor));
        }
    }
}
