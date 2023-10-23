using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Bearded.Utilities.Linq;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Content.Behaviors;

/*
 * Current:
 *
 * ComponentFactory<TParams>
 *   Func<TParams, ISimulationComponent> <-- factory for a given type
 *   TParams
 *
 * Want:
 *
 * ComponentFactory<TParams>
 *   Func<TParams, Array<string>, ISimulationComponent> <-- factory for a given type
 *   TParams
 *   Array<string>
 *
 * Problems:
 *
 * - The func is made once per type without knowing the params or keys, so to inject keys into the component, we need to
 *   expand the func with additional parameters
 * - However, there to pull everything together, we need to create the proper generic func type as well as call .Invoke
 *   with it, which will become problematic if the number of parameters in the func is hard to predict
 *
 * Possible solutions:
 *
 * 1. Change _all_ the behaviours to pass in the original model object as second parameter to the lambda, so it is
 *    always accessible. The factory will then also track that original serialized model so it can access its fields.
 * 2. Migrate to source generation for some of this so the code at least becomes readable. Not necessarily a solution
 *    though, because it still has the same problems.
 *
 */

sealed class BehaviorFactories<TBehaviorTemplate, TBehaviorAttribute, TEmptyConstructorParameters>
    where TBehaviorTemplate : IBehaviorTemplate
    where TBehaviorAttribute : Attribute, IBehaviorAttribute
{
    #region Public interface

    public IDictionary<string, Type> ParameterTypesById { get; private set; } = null!;

    public object CreateBehaviorFactory(TBehaviorTemplate template) => tryMakeBehaviorFactory(template);

    public BehaviorFactories(Type behaviorInterface, MethodInfo factoryFactoryMethodInfo)
    {
        this.behaviorInterface = behaviorInterface;
        this.factoryFactoryMethodInfo = factoryFactoryMethodInfo;
    }

    #endregion

    #region Private permanent storage

    private static Type thisType => typeof(BehaviorFactories<,,>).MakeGenericType(
        typeof(TBehaviorTemplate),
        typeof(TBehaviorAttribute),
        typeof(TEmptyConstructorParameters));

    private readonly Type behaviorInterface;
    private readonly Type emptyConstructorParameterType = typeof(TEmptyConstructorParameters);
    private readonly MethodInfo factoryFactoryMethodInfo;
    private ImmutableArray<PropertyInfo> extraProperties = ImmutableArray<PropertyInfo>.Empty;

    private bool initialized;

    private readonly Dictionary<string /* id */, Type /* parameter */> parametersById =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string /* id */, object /* factoryFactory */> factoryFactoriesById =
        new(StringComparer.OrdinalIgnoreCase);

    #endregion

    #region Implementation

    #region Initialisation

    public void Initialize()
    {
        if (initialized)
        {
            throw new InvalidOperationException("Factories can only be initialised once.");
        }

        initialized = true;

        extraProperties = typeof(TBehaviorTemplate).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.Name != nameof(IBehaviorTemplate.Id) && p.Name != nameof(IBehaviorTemplate.Parameters))
            .Select(p => behaviorInterface.GetProperty(p.Name) is { } p2 && p2.PropertyType == p.PropertyType ? p2 : null)
            .NotNull()
            .ToImmutableArray();

        var knownBehaviors = Assembly.GetExecutingAssembly().GetTypes()
            .Select(t => (type: t, attribute: t.GetCustomAttribute<TBehaviorAttribute>(false)))
            .Where(t => t.attribute != null)
            .Select(t => (t.attribute.Id, t.type))
            .ToList();

        foreach (var subject in knownBehaviors)
            register(subject);

        ParameterTypesById = ImmutableDictionary.CreateRange(parametersById);
    }

    private void register((string id, Type type) behavior)
        => register(behavior.id, behavior.type);

    private void register(string id, Type behaviorType)
    {
        var parameterType = constructorParameterTypeOf(behaviorType);

        var registeredBehavior = tryRegisterBehavior(id, behaviorType, parameterType);

        if (!registeredBehavior)
        {
            throw new Exception($"Failed to register behavior type '{behaviorType}'.");
        }
    }

    private bool tryRegisterBehavior(string id, Type behavior, Type parameters)
    {
        if (!behaviorInterface.IsAssignableFrom(behavior))
        {
            return false;
        }

        registerFactoryFactory(id, behavior, parameters);
        return true;
    }

    private void registerFactoryFactory(
        string id, Type behavior, Type parameters)
    {
        var factoryFactory = makeFactoryFactory(behavior, parameters);

        factoryFactoriesById[id] = factoryFactory;
        parametersById[id] = parameters;
    }

    private object makeFactoryFactory(Type behavior, Type parameters)
    {
        var typedMaker = factoryFactoryMethodInfo.MakeGenericMethod(parameters);

        var paramParameter = Expression.Parameter(parameters);
        var modelParameter = Expression.Parameter(typeof(TBehaviorTemplate));

        var newExpression = parameters == emptyConstructorParameterType
            ? Expression.New(constructorOf(behavior))
            : Expression.New(constructorOf(behavior), paramParameter);

        var memberBindings = createMemberBindings(modelParameter);
        Expression constructorBody = memberBindings.IsDefaultOrEmpty
            ? newExpression
            : Expression.MemberInit(newExpression, memberBindings);

        var constructor = Expression.Lambda(constructorBody, paramParameter, modelParameter);
        Console.WriteLine(constructor.ToString());
        var compiledConstructor = constructor.Compile();

        // returns Func<TParameters, TBehaviorTemplate, object>
        return typedMaker.Invoke(null, new object[] {compiledConstructor});
    }

    private ImmutableArray<MemberBinding> createMemberBindings(Expression modelParameter)
    {
        return extraProperties.Select(property =>
        {
            var propertyToCopy =
                modelParameter.Type.GetProperty(property.Name) ?? throw new InvalidOperationException();
            var memberAccess = Expression.Property(modelParameter, propertyToCopy);
            return (MemberBinding) Expression.Bind(property, memberAccess);
        }).ToImmutableArray();
    }

    private Type constructorParameterTypeOf(Type behaviorType)
    {
        var constructor = constructorOf(behaviorType);
        var constructorParameters = constructor.GetParameters();

        State.Satisfies(constructorParameters.Length <= 1,
            "Behaviour constructor should have exactly zero or one parameter.");

        return constructorParameters.Length == 0
            ? emptyConstructorParameterType
            : constructorParameters[0].ParameterType;
    }

    private static ConstructorInfo constructorOf(Type type)
    {
        var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

        State.Satisfies(constructors.Length == 1,
            "Behaviours should have exactly one public constructor.");

        return constructors[0];
    }

    #endregion

    #region Fetching

    private readonly MethodInfo tryMakeBehaviorFactoryMethodInfo = thisType
        .GetMethod(nameof(tryMakeBehaviorFactoryGeneric), BindingFlags.Instance | BindingFlags.NonPublic);

    private object tryMakeBehaviorFactory(TBehaviorTemplate template)
    {
        var id = template.Id;
        var parameterData = template.Parameters;
        var parameterType = parametersById[id];

        if (parameterType == emptyConstructorParameterType)
        {
            State.Satisfies(parameterData == null, $"[{id}] Expected null, but parameter data was {parameterData}");
            parameterData = default(TEmptyConstructorParameters);
        }
        else
        {
            State.Satisfies(
                parameterType.IsInstanceOfType(parameterData),
                $"[{id}] Expected instance of {parameterData}, but {parameterType} is not.");
        }

        var tryMakeFactory = tryMakeBehaviorFactoryMethodInfo.MakeGenericMethod(parameterType);

        return tryMakeFactory.Invoke(this, new[] { id, parameterData, template });
    }

    private object tryMakeBehaviorFactoryGeneric<TParameters>(
        string id, TParameters parameters, TBehaviorTemplate template)
    {
        var factoryFactory = factoryFactoriesById[id];

        var typedFactoryFactory = (Func<TParameters, TBehaviorTemplate, object>) factoryFactory;

        return typedFactoryFactory?.Invoke(parameters, template);
    }

    #endregion

    #endregion
}
