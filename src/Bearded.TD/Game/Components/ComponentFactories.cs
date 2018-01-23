using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Components.Generic;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.Components
{
    static class ComponentFactories
    {
        #region Configurable initialisation

        // TODO: Put this info into attributes

        // ReSharper disable once MemberInitializerValueIgnored
        private static readonly Type[] knownComponentOwners =
        {
            typeof(Building),
            typeof(BuildingGhost),
            typeof(BuildingPlaceholder),
        };

        private static readonly Dictionary<string, Type> knownComponents = new Dictionary<string, Type>
        {
            { "turret", typeof(Turret) },
            { "tileVisibility", typeof(TileVisibility<>) },
        };

        #endregion

        #region Public interface

        public static Type ParameterTypeForComponent(string id) => parametersForComponentIds[id];
        public static IDictionary<string, Type> ParameterTypesForComponentsById { get; }

        public static IComponentFactory<TOwner> ComponentFactory<TOwner, TParameters>(string id, TParameters parameters)
        {
            var factoryFactories = componentFactoryFactoriesForComponentIds[id];

            var typedFactoryFactory = (ComponentFactoryFactory<TOwner, TParameters>)factoryFactories[typeof(TOwner)];

            return typedFactoryFactory(parameters);
        }

        public static IBuildingComponentFactory CreateBuildingComponentFactory(string id, object parameters)
        {
            var factoryFactories = componentFactoryFactoriesForComponentIds[id];

            factoryFactories.TryGetValue(typeof(Building), out var forBuilding);
            factoryFactories.TryGetValue(typeof(BuildingGhost), out var forGhost);
            factoryFactories.TryGetValue(typeof(BuildingPlaceholder), out var forPlaceholder);

            // TODO: precompile these after registering for faster return

            return new BuildingComponentFactory<>(parameters);
        }

        #endregion

        #region Private permanent storage

        private static readonly Dictionary<string /* id */, Type /* parameter */> parametersForComponentIds = new Dictionary<string, Type>();
        private static readonly Dictionary<string /* id */, Dictionary<Type /* owner */, object /* factoryFactory */>>
            componentFactoryFactoriesForComponentIds = new Dictionary<string, Dictionary<Type, object>>();

        #endregion

        #region Implementation

        private delegate ComponentFactory<TOwner, TParameters> ComponentFactoryFactory<TOwner, TParameters>(TParameters parameters);

        static ComponentFactories()
        {
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

            parametersForComponentIds.Add(id, parameters);
            if (!componentFactoryFactoriesForComponentIds.TryGetValue(id, out var factoryFactoriesForId))
            {
                factoryFactoriesForId = new Dictionary<Type, object>();
                componentFactoryFactoriesForComponentIds.Add(id, factoryFactoriesForId);
            }

            factoryFactoriesForId.Add(owner, factoryFactory);
        }

        private static readonly MethodInfo makeFactoryFactoryMethodInfo = typeof(ComponentFactories).GetMethod(nameof(makeFactoryFactory));

        private static object makeFactoryFactory(Type component, Type owner, Type parameters)
        {
            var typedMaker = makeFactoryFactoryMethodInfo.MakeGenericMethod(owner, parameters);

            var parameter = Expression.Parameter(parameters);
            var constructorBody = Expression.New(component.GetConstructors(BindingFlags.Public)[0], parameter);
            var constructor = Expression.Lambda(constructorBody, parameter);
            var compiledConstructor = constructor.Compile();

            // returns ComponentFactoryFactory<TOwner, TParameters>
            return typedMaker.Invoke(null, new object[] {compiledConstructor});
        }

        private static object makeFactoryFactory<TOwner, TParameters>(Func<TParameters, IComponent<TOwner>> constructor)
        {
            return (ComponentFactoryFactory<TOwner, TParameters>)(p => new ComponentFactory<TOwner, TParameters>(p, constructor));
        }
        
        private static Type constructorParameterTypeOf(Type componentType)
        {
            var constructors = componentType.GetConstructors(BindingFlags.Public);

            DebugAssert.State.Satisfies(constructors.Length == 1, "Components should have exactly one public constructor.");

            var constructor = constructors[0];
            var constructorParameters = constructor.GetParameters();

            DebugAssert.State.Satisfies(constructorParameters.Length == 1, "Component constructor should have exactly one parameter.");

            return constructorParameters[0].ParameterType;
        }

        #endregion
    }
}