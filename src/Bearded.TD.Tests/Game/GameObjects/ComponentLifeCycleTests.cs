using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.Drawing.Particles;
using Bearded.TD.Game.Simulation.Elements;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Sounds;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Testing.Components;
using Bearded.Utilities.Linq;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Bearded.TD.Tests.Game.GameObjects;

public sealed class ComponentLifeCycleTests
{
    // Components that won't work in the limited test bed we have.
    private static readonly ImmutableHashSet<Type> nonFunctionalComponents = ImmutableHashSet.Create(
        typeof(ArcRenderer), // Expects a target property to always be present.
        typeof(Child), // Attempts to instantiate a blueprint on activate
        typeof(Foundation), // Requests hardcoded access to a blueprint.
        typeof(GhostBuildingRenderer), // Requests hardcoded access to a blueprint.
        typeof(HitTargetOnActivate), // Expects a target property to always be present.
        typeof(ProjectileEmitter), // Attempts to instantiate a blueprint on added.
        typeof(SpawnObjectOnBuildingPlaced), // Expects building state to always be present.
        typeof(ThrowOnActivate), // The whole point is to prevent part of the lifecycle.
        typeof(Turret) // Attempts to instantiate a blueprint on added.
    );

    // Components that access assets in OnActivate.
    private static readonly ImmutableHashSet<Type> assetAccessingBlueprints = ImmutableHashSet.Create(
        typeof(AnimatedSprite), // Attempts to instantiate a sprite on activate.
        typeof(Child), // Attempts to instantiate a sprite on activate.
        typeof(Draw), // Attempts to instantiate a sprite on activate.
        typeof(EnemyIcon), // Attempts to instantiate a sprite on activate.
        typeof(LoopSound), // Attempts to play a sound on activate.
        typeof(MuzzleFlash), // Attempts to instantiate a sprite on activate.
        typeof(ParticleSystem), // Attempts to instantiate a sprite on activate.
        typeof(PlaySoundOnActivate), // Attempts to play a sound on activate.
        typeof(SpawnOnCreate), // Attempts to access particles on activate.
        typeof(Sprite), // Attempts to instantiate a sprite on activate.
        typeof(TargetEnemiesInRange), // Attempts to access the weapon state on activate.
        typeof(Trail) // Attempts to instantiate a sprite on activate.
    );

    // Components that can never be removed from their owner.
    private static readonly ImmutableHashSet<Type> permanentComponents = ImmutableHashSet.Create(
        typeof(BuildingStateManager),
        typeof(Health)
    );

    public static IEnumerable<object[]> GetAllComponents()
    {
        return getAllComponentTypes().Select(type => new []{ type });
    }

    private static IEnumerable<Type> getAllComponentTypes()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(isInstantiableComponent);
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
        if (nonFunctionalComponents.Contains(type) ||
            assetAccessingBlueprints.Contains(type) ||
            makeInstance(type, out var dependencies) is not { } component)
        {
            return;
        }

        var componentTestBed = ComponentTestBed.CreateOrphaned();

        // Lifecycle
        foreach (var dep in dependencies)
        {
            componentTestBed.AddComponent(dep);
        }
        componentTestBed.AddComponent(component);
        componentTestBed.AddToGameState();
        if (!permanentComponents.Contains(type)) componentTestBed.RemoveComponent(component);
    }

    [Theory]
    [MemberData(nameof(GetAllComponents))]
    public void LifeCycleWithoutActivateThrowsNoErrors(Type type)
    {
        if (nonFunctionalComponents.Contains(type) || makeInstance(type, out _) is not { } component)
        {
            return;
        }

        var componentTestBed = ComponentTestBed.CreateOrphaned();

        // Lifecycle
        componentTestBed.AddComponent(component);
        if (!permanentComponents.Contains(type)) componentTestBed.RemoveComponent(component);
    }

    private IComponent? makeInstance(Type type, out IEnumerable<IComponent> dependencies)
    {
        dependencies = ImmutableArray<IComponent>.Empty;

        if (type.ContainsGenericParameters)
        {
            output.WriteLine($"WARN: {type} is generic. Skipping.");
            return null;
        }

        var constructors = type.GetConstructors();

        if (constructors.Length != 1)
        {
            output.WriteLine($"WARN: {type} has more than one constructor. Skipping.");
            return null;
        }

        var injectedTypes = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(t => t.GetCustomAttribute<InjectAttribute>() is not null)
            .Select(f => f.FieldType);
        dependencies = injectedTypes
            .Select(resolveDependency)
            .SelectMany(t =>
            {
                if (t == null)
                {
                    return Enumerable.Empty<IComponent>();
                }

                var c = makeInstance(t, out var transitiveDependencies);
                return c.Yield().Concat(transitiveDependencies);
            })
            .ToImmutableArray();

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

        var parameter = parameters[0];
        if (!parameter.ParameterType.IsAssignableTo(typeof(IParametersTemplate)))
        {
            output.WriteLine(
                $"WARN: {type} has constructor with parameter that does not implement parameters template. Skipping.");
            return null;
        }

        var parameterInstance = mockedParameters(parameter.ParameterType);

        if (instantiatedComponentWithParameter(constructor, parameter.ParameterType, parameterInstance)
            is IComponent component)
        {
            return component;
        }

        output.WriteLine($"WARN: could not instantiate {type}. Skipping.");
        return null;
    }

    private static Type? resolveDependency(Type type)
    {
        return isInstantiableComponent(type)
            ? type
            : getAllComponentTypes().FirstOrDefault(t => t.IsAssignableTo(type));
    }

    private static bool isInstantiableComponent(Type type)
    {
        return type.IsAssignableTo(typeof(IComponent)) && type.IsClass && !type.IsAbstract;
    }

    private static object mockedParameters(Type type)
    {
        var methodCallEx = Expression.Call(typeof(ComponentLifeCycleTests), nameof(mockedParameters), new[] { type });
        var lambdaEx = Expression.Lambda(methodCallEx);
        return lambdaEx.Compile().DynamicInvoke()!;
    }

    private static object mockedParameters<T>() where T : class, IParametersTemplate<T>
    {
        var mock = new Mock<T>();
        mock.Setup(m => m.CreateModifiableInstance()).Returns(mock.Object);
        mock.SetReturnsDefault((ITrigger) new EmptyTrigger());
        return mock.Object;
    }

    private static object? instantiatedComponentWithParameter(
        ConstructorInfo constructor, Type parameterParameterType, object parameterInstance)
    {
        var parameterParameter = Expression.Parameter(parameterParameterType);
        var newComponentExpression =
            Expression.Lambda(Expression.New(constructor, parameterParameter), parameterParameter);
        var componentInstance = newComponentExpression.Compile().DynamicInvoke(parameterInstance);
        return componentInstance;
    }
}
