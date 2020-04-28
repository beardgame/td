using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Bearded.TD.Game.Components;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;

namespace Bearded.TD.Content.Serialization.Models
{
    // TDomainSpecificFactory -> TDomainSpecificBehavior -> TOwner

    // TDomainSpecificFactory<TOwner, TParameters>
    // - parameters
    // - a way to go from parameters to TDomainSpecificBehavior

    sealed class BehaviorFactories<TBehaviorTemplate, TBehaviorAttribute, TOwnerAttribute>
        where TBehaviorTemplate : IBehaviorTemplate
        where TBehaviorAttribute : Attribute, IBehaviorAttribute
        where TOwnerAttribute : Attribute
    {
        #region Public interface

        public IDictionary<string, Type> ParameterTypesById { get; private set; }

        public IBehaviorFactory<TOwner> CreateBehaviorFactory<TOwner>(TBehaviorTemplate template)
            => tryMakeBehaviorFactory<TOwner>(template);

        public BehaviorFactories(Type behaviorInterface)
        {
            this.behaviorInterface = behaviorInterface;
        }

        #endregion

        #region Private permanent storage

        private static Type thisType =>
            typeof(BehaviorFactories<,,>)
                .MakeGenericType(typeof(TBehaviorTemplate), typeof(TBehaviorAttribute), typeof(TOwnerAttribute));

        private readonly Type behaviorInterface;

        private bool initialized;

        private readonly Dictionary<string /* id */, Type /* parameter */> parametersById
            = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string /* id */, Dictionary<Type /* owner */, object /* factoryFactory */>>
            factoryFactoriesById = new Dictionary<string, Dictionary<Type, object>>(StringComparer.OrdinalIgnoreCase);

        #endregion

        #region Implementation

        #region Initialisation

        private delegate BehaviorFactory<TBehaviorInterface, TParameters>
            BehaviorFactoryFactory<TBehaviorInterface, TParameters>(TParameters parameters);

        public void Initialize()
        {
            if (initialized)
            {
                throw new InvalidOperationException("Factories can only be initialised once.");
            }

            initialized = true;

            var knownBehaviors = Assembly.GetExecutingAssembly().GetTypes()
                .Select(t => (type: t, attribute: t.GetCustomAttribute<TBehaviorAttribute>(false)))
                .Where(t => t.attribute != null)
                .Select(t => (t.attribute.Id, t.type))
                .ToList();

            var knownOwners = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttribute<TOwnerAttribute>() != null)
                .ToList();

            foreach (var subject in knownBehaviors)
                register(subject, knownOwners);

            ParameterTypesById = ImmutableDictionary.CreateRange(parametersById);
        }

        private void register((string id, Type type) behavior, List<Type> knownOwners)
            => register(behavior.id, behavior.type, knownOwners);

        private void register(string id, Type behaviorType, List<Type> knownOwners)
        {
            var parameterType = constructorParameterTypeOf(behaviorType);

            var tryRegister = behaviorType.IsGenericType
                ? (Func<string, Type, Type, Type, bool>) tryRegisterGenericBehavior
                : tryRegisterBehavior;

            var registeredBehavior = false;
            foreach (var owner in knownOwners)
            {
                registeredBehavior = tryRegister(id, behaviorType, owner, parameterType) || registeredBehavior;
            }

            if (!registeredBehavior)
            {
                throw new Exception($"Failed to register behavior type '{behaviorType}'.");
            }
        }

        private bool tryRegisterGenericBehavior(string id, Type behavior, Type owner, Type parameters)
        {
            Type typedBehavior = null;

            try
            {
                typedBehavior = behavior.MakeGenericType(owner);
            }
            catch (ArgumentException)
            {
            }

            return typedBehavior != null && tryRegisterBehavior(id, typedBehavior, owner, parameters);
        }

        private bool tryRegisterBehavior(string id, Type behavior, Type owner, Type parameters)
        {
            var concreteInterface = behaviorInterface.MakeGenericType(owner);

            if (!concreteInterface.IsAssignableFrom(behavior))
            {
                return false;
            }

            registerFactoryFactory(id, behavior, owner, parameters);
            return true;
        }

        private void registerFactoryFactory(
            string id, Type behavior, Type owner, Type parameters)
        {
            var factoryFactory = makeFactoryFactory(behavior, owner, parameters);

            parametersById[id] = parameters; // will write multiple times for multi-use behaviours ¯\_(ツ)_/¯
            if (!factoryFactoriesById.TryGetValue(id, out var factoryFactoriesByOwnerForId))
            {
                factoryFactoriesByOwnerForId = new Dictionary<Type, object>();
                factoryFactoriesById.Add(id, factoryFactoriesByOwnerForId);
            }

            factoryFactoriesByOwnerForId.Add(owner, factoryFactory);
        }

        private readonly MethodInfo makeFactoryFactoryMethodInfo = thisType
            .GetMethod(nameof(makeFactoryFactoryGeneric), BindingFlags.NonPublic | BindingFlags.Static);

        private readonly Type emptyConstructorParameterType = typeof(VoidParameters);

        private object makeFactoryFactory(Type behavior, Type owner, Type parameters)
        {
            var typedMaker = makeFactoryFactoryMethodInfo.MakeGenericMethod(owner, parameters);

            var parameter = Expression.Parameter(parameters);

            var constructorBody = parameters == emptyConstructorParameterType
                ? Expression.New(constructorOf(behavior))
                : Expression.New(constructorOf(behavior), parameter);
            var constructor = Expression.Lambda(constructorBody, parameter);
            var compiledConstructor = constructor.Compile();

            // returns BehaviorFactoryFactory<TOwner, TParameters>
            return typedMaker.Invoke(null, new object[] {compiledConstructor});
        }

        private static object makeFactoryFactoryGeneric<TOwner, TParameters>(
            Func<TParameters, IBehavior<TOwner>> constructor)
            where TParameters : IParametersTemplate<TParameters>
        {
            return (BehaviorFactoryFactory<TOwner, TParameters>) (p =>
                new BehaviorFactory<TOwner, TParameters>(p, constructor));
        }

        private Type constructorParameterTypeOf(Type behaviorType)
        {
            var constructor = constructorOf(behaviorType);
            var constructorParameters = constructor.GetParameters();

            DebugAssert.State.Satisfies(constructorParameters.Length <= 1,
                "Behaviour constructor should have exactly zero or one parameter.");

            return constructorParameters.Length == 0
                ? emptyConstructorParameterType
                : constructorParameters[0].ParameterType;
        }

        private static ConstructorInfo constructorOf(Type type)
        {
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            DebugAssert.State.Satisfies(constructors.Length == 1,
                "Behaviours should have exactly one public constructor.");

            return constructors[0];
        }

        #endregion

        #region Fetching

        private readonly MethodInfo tryMakeBehaviorFactoryMethodInfo = thisType
            .GetMethod(nameof(tryMakeBehaviorFactoryGeneric), BindingFlags.Instance | BindingFlags.NonPublic);

        private IBehaviorFactory<TOwner> tryMakeBehaviorFactory<TOwner>(TBehaviorTemplate template)
        {
            var id = template.Id;
            var parameterData = template.Parameters;
            var parameterType = parametersById[id];

            if (parameterType == emptyConstructorParameterType)
            {
                DebugAssert.State.Satisfies(parameterData == null);
                parameterData = VoidParameters.Instance;
            }
            else
            {
                DebugAssert.State.Satisfies(parameterType.IsInstanceOfType(parameterData));
            }

            var tryMakeFactory = tryMakeBehaviorFactoryMethodInfo.MakeGenericMethod(typeof(TOwner), parameterType);

            return (IBehaviorFactory<TOwner>)tryMakeFactory.Invoke(this, new[] { id, parameterData });
        }

        private object tryMakeBehaviorFactoryGeneric<TOwner, TParameters>(string id, TParameters parameters)
            where TParameters : IParametersTemplate<TParameters>
        {
            var factoryFactories = factoryFactoriesById[id];

            factoryFactories.TryGetValue(typeof(TOwner), out var factoryFactory);

            var typedFactoryFactory = (BehaviorFactoryFactory<TOwner, TParameters>) factoryFactory;

            return typedFactoryFactory?.Invoke(parameters);
        }

        #endregion

        #endregion

    }

    interface IBehavior<TOwner>
    {

    }

    interface IBehaviorAttribute
    {
        string Id { get; }
    }

    interface IBehaviorTemplate
    {
        string Id { get; }
        object Parameters { get; }
    }

    interface IBehaviorFactory<TOwner>
    {
        IBehavior<TOwner> Create();
    }

    sealed class BehaviorFactory<TOwner, TParameters> : IBehaviorFactory<TOwner>
    {
        private readonly TParameters parameters;
        private readonly Func<TParameters, IBehavior<TOwner>> factory;

        public BehaviorFactory(TParameters parameters, Func<TParameters, IBehavior<TOwner>> factory)
        {
            this.parameters = parameters;
            this.factory = factory;
        }

        public IBehavior<TOwner> Create() => factory(parameters);

        // TODO(Tom): remove the need for this
        public TParameters UglyWayToAccessParameters => parameters;
    }
}
