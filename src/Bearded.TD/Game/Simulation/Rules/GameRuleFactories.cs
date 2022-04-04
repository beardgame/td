using System;
using System.Collections.Generic;
using System.Reflection;
using Bearded.TD.Content.Behaviors;
using IGameRuleModel = Bearded.TD.Content.Serialization.Models.IGameRule;

namespace Bearded.TD.Game.Simulation.Rules;

static class GameRuleFactories
{
    private static readonly MethodInfo makeFactoryFactoryMethodInfo = typeof(GameRuleFactories)
        .GetMethod(nameof(makeFactoryFactoryGeneric), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly BehaviorFactories<IGameRuleModel, GameRuleAttribute, VoidParameters> factories =
        new(typeof(IGameRule), makeFactoryFactoryMethodInfo);

    public static void Initialize() => factories.Initialize();

    public static IDictionary<string, Type> ParameterTypesForComponentsById => factories.ParameterTypesById;

    public static IGameRuleFactory CreateGameRuleFactory(IGameRuleModel template) =>
        (factories.CreateBehaviorFactory(template) as IGameRuleFactory)!;

    private static object makeFactoryFactoryGeneric<TParameters>(
        Func<TParameters, IGameRule> constructor)
    {
        return (Func<TParameters, object>) (p => new GameRuleFactory<TParameters>(p, constructor));
    }
}
