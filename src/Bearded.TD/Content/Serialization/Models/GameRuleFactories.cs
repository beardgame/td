using System;
using System.Collections.Generic;
using System.Reflection;
using Bearded.TD.Content.Behaviors;
using Bearded.TD.Game.Simulation.Rules;
using ISimulationGameRule = Bearded.TD.Game.Simulation.Rules.IGameRule;

namespace Bearded.TD.Content.Serialization.Models;

static class GameRuleFactories
{
    private static readonly MethodInfo makeFactoryFactoryMethodInfo = typeof(GameRuleFactories)
        .GetMethod(nameof(makeFactoryFactoryGeneric), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly BehaviorFactories<IGameRule, GameRuleAttribute, VoidParameters> factories =
        new(typeof(ISimulationGameRule), makeFactoryFactoryMethodInfo);

    public static void Initialize() => factories.Initialize();

    public static IDictionary<string, Type> ParameterTypesForComponentsById => factories.ParameterTypesById;

    public static IGameRuleFactory CreateGameRuleFactory(IGameRule template) =>
        (factories.CreateBehaviorFactory(template) as IGameRuleFactory)!;

    private static object makeFactoryFactoryGeneric<TParameters>(
        Func<TParameters, IGameRule, ISimulationGameRule> constructor)
    {
        return (Func<TParameters, IGameRule, object>) ((p, m) => new GameRuleFactory<TParameters>(p, m, constructor));
    }

    private sealed class GameRuleFactory<TParameters> : IGameRuleFactory
    {
        private readonly TParameters parameters;
        private readonly IGameRule model;
        private readonly Func<TParameters, IGameRule, ISimulationGameRule> factory;

        public GameRuleFactory(
            TParameters parameters, IGameRule model, Func<TParameters, IGameRule, ISimulationGameRule> factory)
        {
            this.parameters = parameters;
            this.model = model;
            this.factory = factory;
        }

        public ISimulationGameRule Create() => factory(parameters, model);
    }
}
