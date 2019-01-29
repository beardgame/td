using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Components;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;

namespace Bearded.TD.Content.Serialization.Models
{
    static class ComponentFactories
    {
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

        private static bool initialised;

        private static readonly Dictionary<string /* id */, Type /* parameter */> parametersForComponentIds
            = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string /* id */, Dictionary<Type /* owner */, object /* factoryFactory */>>
            componentFactoryFactoriesForComponentIds = new Dictionary<string, Dictionary<Type, object>>(
                StringComparer.OrdinalIgnoreCase);

        #endregion

        #region Implementation

        #region Initialisation

        private delegate ComponentFactory<TOwner, TParameters> ComponentFactoryFactory<TOwner, TParameters>(TParameters parameters)
            where TParameters : IParametersTemplate<TParameters>;

        public static void Initialize()
        {
            if (initialised)
                throw new InvalidOperationException("Component factories can only be initialised once.");

            initialised = true;

            var knownComponents = Assembly.GetExecutingAssembly().GetTypes()
                .Select(t => (type: t, attribute: CustomAttributeExtensions.GetCustomAttribute<ComponentAttribute>((MemberInfo) t, false)))
                .Where(t => t.attribute != null)
                .Select(t => (t.attribute.Id, t.type))
                .ToList();

            var componentOwners = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => CustomAttributeExtensions.GetCustomAttribute<ComponentOwnerAttribute>((MemberInfo) t) != null)
                .ToList();
            
            foreach (var component in knownComponents)
                register(component, componentOwners);

            ParameterTypesForComponentsById = new ReadOnlyDictionary<string, Type>(parametersForComponentIds);
        }
        
        private static void register((string id, Type type) component, List<Type> componentOwners)
            => register(component.id, component.type, componentOwners);

        private static void register(string id, Type componentType, List<Type> componentOwners)
        {
            var parameterType = constructorParameterTypeOf(componentType);

            var tryRegister = componentType.IsGenericType
                ? (Func<string, Type, Type, Type, bool>)tryRegisterGenericComponent
                : tryRegisterComponent;

            var registeredComponent = false;
            foreach (var owner in componentOwners)
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

        private static readonly Type emptyConstructorParameterType = typeof(VoidParameters);

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
            where TParameters : IParametersTemplate<TParameters>
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

            if (parameterType == emptyConstructorParameterType)
            {
                DebugAssert.State.Satisfies(parameterData == null);
                parameterData = VoidParameters.Instance;
            }
            else
            {
                DebugAssert.State.Satisfies(parameterType.IsInstanceOfType(parameterData));
            }

            var tryMakeFactory = tryMakeComponentFactoryMethodInfo.MakeGenericMethod(typeof(TOwner), parameterType);

            return (IComponentFactory<TOwner>)tryMakeFactory.Invoke(null, new[] { id, parameterData });
        }

        private static IComponentFactory<TOwner> tryMakeComponentFactoryGeneric<TOwner, TParameters>(string id, TParameters parameters)
            where TParameters : IParametersTemplate<TParameters>
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