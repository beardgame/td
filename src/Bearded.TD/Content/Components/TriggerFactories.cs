using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Utilities;

namespace Bearded.TD.Content.Components;

sealed class TriggerFactories
{
    private delegate ITrigger TriggerFactory();

    private static ImmutableDictionary<string, TriggerFactory>? factories;

    public static void Initialize()
    {
        if (factories != null)
        {
            throw new InvalidOperationException("Factories can only be initialized once.");
        }

        factories = Assembly.GetExecutingAssembly().GetTypes()
            .Select(t => (type: t, attribute: t.GetCustomAttribute<TriggerAttribute>(false)))
            .Where(t => t.attribute != null)
            .ToImmutableDictionary(t => t.attribute.Id, t => makeTriggerFactory(t.type));
    }

    public static ITrigger CreateForId(string id)
    {
        if (factories == null)
        {
            throw new InvalidOperationException("Cannot create triggers before initializing trigger factories.");
        }

        if (!factories.TryGetValue(id, out var factory))
        {
            throw new KeyNotFoundException($"Cannot find trigger factory for id {id}.");
        }

        return factory();
    }

    private static TriggerFactory makeTriggerFactory(Type type)
    {
        var genericTrigger = typeof(Trigger<>).MakeGenericType(type);
        var genericTriggerCtor = constructorOf(genericTrigger);

        var constructorBody = Expression.New(genericTriggerCtor);
        var constructor =
            Expression.Lambda(typeof(TriggerFactory), constructorBody, Array.Empty<ParameterExpression>());
        return (TriggerFactory) constructor.Compile();
    }

    private static ConstructorInfo constructorOf(Type type)
    {
        var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

        DebugAssert.State.Satisfies(constructors.Length == 1,
            "Trigger should have exactly one public constructor.");

        return constructors[0];
    }
}
