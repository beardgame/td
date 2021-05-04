using System;
using System.Collections.Generic;
using System.Reflection;
using Bearded.TD.Content.Behaviors;
using Bearded.TD.Content.Serialization.Models;

namespace Bearded.TD.Game.Simulation.Rules
{
    static class GameRuleFactories
    {
        private static readonly MethodInfo makeFactoryFactoryMethodInfo = typeof(GameRuleFactories)
            .GetMethod(nameof(makeFactoryFactoryGeneric), BindingFlags.NonPublic | BindingFlags.Static)!;

        private static readonly BehaviorFactories<IGameRule, GameRuleAttribute, GameRuleOwnerAttribute, VoidParameters> factories =
            new(typeof(IGameRule<>), makeFactoryFactoryMethodInfo);

        public static void Initialize() => factories.Initialize();

        public static IDictionary<string, Type> ParameterTypesForComponentsById => factories.ParameterTypesById;

        public static IGameRuleFactory<TOwner> CreateGameRuleFactory<TOwner>(IGameRule template) =>
            (factories.CreateBehaviorFactory<GameState>(template) as IGameRuleFactory<TOwner>)!;

        private static object makeFactoryFactoryGeneric<TOwner, TParameters>(
            Func<TParameters, IGameRule<TOwner>> constructor)
        {
            return (Func<TParameters, object>) (p => new GameRuleFactory<TOwner, TParameters>(p, constructor));
        }
    }
}
