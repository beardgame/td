using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Content.Behaviors
{
    sealed class BehaviorFactories<TBehaviorTemplate, TBehaviorAttribute, TOwnerAttribute, TEmptyConstructorParameters>
        where TBehaviorTemplate : IBehaviorTemplate
        where TBehaviorAttribute : Attribute, IBehaviorAttribute
        where TOwnerAttribute : Attribute
    {
        #region Public interface

        public IDictionary<string, Type> ParameterTypesById { get; private set; }

        public object CreateBehaviorFactory<TOwner>(TBehaviorTemplate template)
            => tryMakeBehaviorFactory<TOwner>(template);

        public BehaviorFactories(Type behaviorInterface, MethodInfo factoryFactoryMethodInfo)
        {
            this.behaviorInterface = behaviorInterface;
            this.factoryFactoryMethodInfo = factoryFactoryMethodInfo;
        }

        #endregion

        #region Private permanent storage

        private static Type thisType => typeof(BehaviorFactories<,,,>).MakeGenericType(
            typeof(TBehaviorTemplate),
            typeof(TBehaviorAttribute),
            typeof(TOwnerAttribute),
            typeof(TEmptyConstructorParameters));

        private readonly Type behaviorInterface;
        private readonly Type emptyConstructorParameterType = typeof(TEmptyConstructorParameters);
        private readonly MethodInfo factoryFactoryMethodInfo;

        private bool initialized;

        private readonly Dictionary<string /* id */, Type /* parameter */> parametersById
            = new(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string /* id */, Dictionary<Type /* owner */, object /* factoryFactory */>>
            factoryFactoriesById = new Dictionary<string, Dictionary<Type, object>>(StringComparer.OrdinalIgnoreCase);

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

        private object makeFactoryFactory(Type behavior, Type owner, Type parameters)
        {
            var typedMaker = factoryFactoryMethodInfo.MakeGenericMethod(owner, parameters);

            var parameter = Expression.Parameter(parameters);

            var constructorBody = parameters == emptyConstructorParameterType
                ? Expression.New(constructorOf(behavior))
                : Expression.New(constructorOf(behavior), parameter);
            var constructor = Expression.Lambda(constructorBody, parameter);
            var compiledConstructor = constructor.Compile();

            // returns Func<TParameters, object>
            return typedMaker.Invoke(null, new object[] {compiledConstructor});
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

        private object tryMakeBehaviorFactory<TOwner>(TBehaviorTemplate template)
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

            var tryMakeFactory = tryMakeBehaviorFactoryMethodInfo.MakeGenericMethod(typeof(TOwner), parameterType);

            return tryMakeFactory.Invoke(this, new[] { id, parameterData });
        }

        private object tryMakeBehaviorFactoryGeneric<TOwner, TParameters>(string id, TParameters parameters)
        {
            var factoryFactories = factoryFactoriesById[id];

            factoryFactories.TryGetValue(typeof(TOwner), out var factoryFactory);

            var typedFactoryFactory = (Func<TParameters, object>) factoryFactory;

            return typedFactoryFactory?.Invoke(parameters);
        }

        #endregion

        #endregion
    }
}
