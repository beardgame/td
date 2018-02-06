using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Components.Generic;
using Bearded.TD.Mods.Serialization.Models;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.Components
{
    static class ComponentFactories
    {
        #region Configurable initialisation

        // TODO: Put this info into attributes

        // ReSharper disable once MemberInitializerValueIgnored
        private static Type[] knownComponentOwners =
        {
            typeof(Building),
            typeof(BuildingGhost),
            typeof(BuildingPlaceholder),
        };

        private static Dictionary<string, Type> knownComponents = new Dictionary<string, Type>
        {
            { "sink", typeof(EnemySink) },
            { "gameOverOnDestroy", typeof(GameOverOnDestroy<>) },
            { "turret", typeof(Turret) },
            { "workerHub", typeof(WorkerHub<>) },
            { "tileVisibility", typeof(TileVisibility<>) },
            { "incomeOverTime", typeof(IncomeOverTime<>) },
        };

        #endregion

        #region Public interface

        public static Type ParameterTypeForComponent(string id) => parametersForComponentIds[id];
        public static IDictionary<string, Type> ParameterTypesForComponentsById { get; private set; }

        public static IComponentFactory<TOwner> CreateComponentFactory<TOwner>(IComponent parameters)
            => tryMakeComponentFactory<TOwner>(parameters);

        public static BuildingComponentFactory CreateBuildingComponentFactory(IBuildingComponent parameters)
        {
            var forBuilding = tryMakeComponentFactory<Building>(parameters);
            var forGhost = tryMakeComponentFactory<BuildingGhost>(parameters);
            var forPlaceholder = tryMakeComponentFactory<BuildingPlaceholder>(parameters);
            
            return new BuildingComponentFactory(parameters, forBuilding, forGhost, forPlaceholder);
        }

        #endregion

        #region Private permanent storage

        private static readonly Dictionary<string /* id */, Type /* parameter */> parametersForComponentIds = new Dictionary<string, Type>();
        private static readonly Dictionary<string /* id */, Dictionary<Type /* owner */, object /* factoryFactory */>>
            componentFactoryFactoriesForComponentIds = new Dictionary<string, Dictionary<Type, object>>();

        #endregion

        #region Implementation

        #region Initialisation

        private delegate ComponentFactory<TOwner, TParameters> ComponentFactoryFactory<TOwner, TParameters>(TParameters parameters);

        public static void Initialize()
        {
            if (knownComponents == null)
                throw new InvalidOperationException("Component factories can only be initialised once.");

            knownComponents.ForEach(register);
            // just cleaning up some stuff we won't need again
            knownComponents = null;
            knownComponentOwners = null;

            ParameterTypesForComponentsById = new ReadOnlyDictionary<string, Type>(parametersForComponentIds);
        }
        
        private static void register(KeyValuePair<string, Type> idAndComponent)
            => register(idAndComponent.Key, idAndComponent.Value);

        private static void register(string id, Type componentType)
        {
            var parameterType = constructorParameterTypeOf(componentType);

            var tryRegister = componentType.IsGenericType
                ? (Func<string, Type, Type, Type, bool>)tryRegisterGenericComponent
                : tryRegisterComponent;

            var registeredComponent = false;
            foreach (var owner in knownComponentOwners)
                registeredComponent = tryRegister(id, componentType, owner, parameterType) || registeredComponent;

            if (!registeredComponent)
                throw new Exception($"Failed to register component type '{componentType}'.");
        }
        
        private static bool tryRegisterGenericComponent(string id, Type component, Type owner, Type parameters)
        {
            Type typedComponent = null;

            try
            {
                typedComponent = component.MakeGenericType(owner);
            }
            catch (ArgumentException) { }

            return typedComponent != null && tryRegisterComponent(id, typedComponent, owner, parameters);
        }

        private static bool tryRegisterComponent(string id, Type component, Type owner, Type parameters)
        {
            var concreteInterface = typeof(IComponent<>).MakeGenericType(owner);

            if (!concreteInterface.IsAssignableFrom(component))
                return false;

            registerFactoryFactory(id, component, owner, parameters);
            return true;
        }

        private static void registerFactoryFactory(string id, Type component, Type owner, Type parameters)
        {
            var factoryFactory = makeFactoryFactory(component, owner, parameters);

            parametersForComponentIds[id] = parameters; // will write multiple times for multi-use components ¯\_(ツ)_/¯
            if (!componentFactoryFactoriesForComponentIds.TryGetValue(id, out var factoryFactoriesForId))
            {
                factoryFactoriesForId = new Dictionary<Type, object>();
                componentFactoryFactoriesForComponentIds.Add(id, factoryFactoriesForId);
            }

            factoryFactoriesForId.Add(owner, factoryFactory);
        }

        private static readonly MethodInfo makeFactoryFactoryMethodInfo = typeof(ComponentFactories)
            .GetMethod(nameof(makeFactoryFactoryGeneric), BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly Type emptyConstructorParameterType = typeof(Bearded.Utilities.Void);

        private static object makeFactoryFactory(Type component, Type owner, Type parameters)
        {
            var typedMaker = makeFactoryFactoryMethodInfo.MakeGenericMethod(owner, parameters);
            
            var parameter = Expression.Parameter(parameters);
            
            var constructorBody = parameters == emptyConstructorParameterType
                ? Expression.New(constructorOf(component))
                : Expression.New(constructorOf(component), parameter);
            var constructor = Expression.Lambda(constructorBody, parameter);
            var compiledConstructor = constructor.Compile();

            // returns ComponentFactoryFactory<TOwner, TParameters>
            return typedMaker.Invoke(null, new object[] {compiledConstructor});
        }

        private static object makeFactoryFactoryGeneric<TOwner, TParameters>(Func<TParameters, IComponent<TOwner>> constructor)
        {
            return (ComponentFactoryFactory<TOwner, TParameters>)(p => new ComponentFactory<TOwner, TParameters>(p, constructor));
        }
        
        private static Type constructorParameterTypeOf(Type componentType)
        {
            var constructor = constructorOf(componentType);
            var constructorParameters = constructor.GetParameters();

            DebugAssert.State.Satisfies(constructorParameters.Length <= 1, "Component constructor should have exactly zero or one parameter.");

            return constructorParameters.Length == 0
                ? emptyConstructorParameterType
                : constructorParameters[0].ParameterType;
        }

        private static ConstructorInfo constructorOf(Type type)
        {
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            DebugAssert.State.Satisfies(constructors.Length == 1, "Components should have exactly one public constructor.");
            
            return constructors[0];
        }


        #endregion

        #region Fetching

        private static readonly MethodInfo tryMakeComponentFactoryMethodInfo = typeof(ComponentFactories)
            .GetMethod(nameof(tryMakeComponentFactoryGeneric), BindingFlags.Static | BindingFlags.NonPublic);

        private static IComponentFactory<TOwner> tryMakeComponentFactory<TOwner>(IComponent parameters)
        {
            var id = parameters.Id;
            var parameterData = parameters.Parameters;
            var parameterType = ParameterTypeForComponent(id);

            DebugAssert.State.Satisfies(parameterData.GetType() == parameterType);

            var tryMakeFactory = tryMakeComponentFactoryMethodInfo.MakeGenericMethod(typeof(TOwner), parameterType);

            return (IComponentFactory<TOwner>)tryMakeFactory.Invoke(null, new[] { id, parameterData });
        }

        private static IComponentFactory<TOwner> tryMakeComponentFactoryGeneric<TOwner, TParameters>(string id, TParameters parameters)
        {
            var factoryFactories = componentFactoryFactoriesForComponentIds[id];

            factoryFactories.TryGetValue(typeof(TOwner), out var factoryFactory);

            var typedFactoryFactory = (ComponentFactoryFactory<TOwner, TParameters>)factoryFactory;

            return typedFactoryFactory?.Invoke(parameters);
        }

        #endregion

        #endregion
    }
}