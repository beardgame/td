using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Testing.Components;
using Xunit;
using Xunit.Abstractions;

namespace Bearded.TD.Tests.Game.GameObjects;

public sealed class ComponentLifeCycleTests
{
    // Components that won't work in the limited test bed we have.
    private static readonly ImmutableHashSet<Type> nonFunctionalComponents = ImmutableHashSet.Create(
        typeof(GhostBuildingRenderer), // Requests hardcoded access to a blueprint.
        typeof(HitTargetOnActivate) // Expects a target property to always be present.
    );

    // Components that can never be removed from their owner.
    private static readonly ImmutableHashSet<Type> permanentComponents = ImmutableHashSet.Create(
        typeof(BuildingStateManager)
    );

    public static IEnumerable<object[]> GetAllComponents()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes().Where(t => t.IsAssignableTo(typeof(IComponent))))
            .Where(type => type.IsClass && !type.IsAbstract)
            .Select(type => new []{ type });
    }

    private readonly ITestOutputHelper output;

    public ComponentLifeCycleTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Theory]
    [MemberData(nameof(GetAllComponents))]
    public void LifeCycleWithActivateThrowsNoErrors(Type type)
    {
        if (nonFunctionalComponents.Contains(type) || makeInstance(type) is not { } component)
        {
            return;
        }

        var componentTestBed = ComponentTestBed.CreateOrphaned();

        // Lifecycle
        componentTestBed.AddComponent(component);
        componentTestBed.AddToGameState();
        if (!permanentComponents.Contains(type)) componentTestBed.RemoveComponent(component);
    }

    [Theory]
    [MemberData(nameof(GetAllComponents))]
    public void LifeCycleWithoutActivateThrowsNoErrors(Type type)
    {
        if (nonFunctionalComponents.Contains(type) || makeInstance(type) is not { } component)
        {
            return;
        }

        var componentTestBed = ComponentTestBed.CreateOrphaned();

        // Lifecycle
        componentTestBed.AddComponent(component);
        if (!permanentComponents.Contains(type)) componentTestBed.RemoveComponent(component);
    }

    private IComponent? makeInstance(Type type)
    {
        var constructors = type.GetConstructors();

        if (constructors.Length != 1)
        {
            output.WriteLine($"WARN: {type} has more than one constructor. Skipping.");
            return null;
        }

        var constructor = constructors[0];
        var parameters = constructor.GetParameters();
        if (parameters.Length == 0)
        {
            return (IComponent) Activator.CreateInstance(type);
        }

        if (parameters.Length != 1)
        {
            output.WriteLine($"WARN: {type} has constructor with more than one parameter. Skipping.");
            return null;
        }

        output.WriteLine($"INFO: {type} has a constructor parameter, which cannot be tested at this time. Skipping.");
        return null;
    }
}
