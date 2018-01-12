using System;
using System.Collections.Generic;
using System.Reflection;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Components.Generic;
using Bearded.TD.Mods.Models;
using Bearded.TD.Mods.Serialization.Models;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Components
{
    static class ComponentFactories
    {
        // "turret", Turret, TurretParameters

        private static void registerAll()
        {
            register("turret", typeof(Turret));
            register("tileVisibility", typeof(TileVisibility<>));
        }

        private static void register(string id, Type componentType)
        {
            var constructors = componentType.GetConstructors(BindingFlags.Public);
            
            DebugAssert.State.Satisfies(constructors.Length == 1, "Components should have exactly one public constructor.");

            var constructor = constructors[0];
            var constructorParameters = constructor.GetParameters();

            DebugAssert.State.Satisfies(constructorParameters.Length == 1, "Component constructor should have exactly one parameter.");

            var parameter = constructorParameters[0];
            var parameterType = parameter.ParameterType;

            if (componentType.IsGenericType)
            {
                Type asBuilding = null;
                Type asGhost = null;
                Type asPlaceholder = null;

                try
                {
                    asBuilding = componentType.MakeGenericType(typeof(Building));
                }
                catch (ArgumentException) { }
                try
                {
                    asGhost = componentType.MakeGenericType(typeof(BuildingGhost));
                }
                catch (ArgumentException) { }
                try
                {
                    asPlaceholder = componentType.MakeGenericType(typeof(BuildingPlaceholder));
                }
                catch (ArgumentException) { }

                if (asBuilding != null || asGhost != null || asPlaceholder != null)
                {
                    
                }
            }
            else
            {
                if (typeof(IComponent<Building>).IsAssignableFrom(componentType))
                {
                    // Func<TComponentParameters, ComponentFactory<Building, TComponentParameters>>

                    makeFactoryFactory(componentType, typeof(Building), parameterType);

                }
                else if (typeof(IComponent<BuildingGhost>).IsAssignableFrom(componentType))
                {

                }
                else if (typeof(IComponent<BuildingPlaceholder>).IsAssignableFrom(componentType))
                {

                }
            }
        }

        private static void makeFactoryFactory(Type component, Type owner, Type paramaters)
        {
            // get method below using reflection
            // call it generically
            // pass a delegate constructor, hope the cast works
        }

        private static void makeFactoryFactory<TComponent, TOwner, TParameters>(Func<TParameters, IComponent<TOwner>> constructor)
        {
            ComponentFactory<TOwner, TParameters> Factory(TParameters p) => new ComponentFactory<TOwner, TParameters>(p, constructor);
        }

        private static Func<IBuildingComponent, IBuildingComponentFactory>
            make<TComponent, TComponentParameter>(Func<TComponentParameter, TComponent> m)
        {

        }
            

        public static Dictionary<string, Func<IBuildingComponent, IBuildingComponentFactory>> Buildings { get; } =
            new Dictionary<string, Func<IBuildingComponent, IBuildingComponentFactory>>
            {
                { "turret", make<Turret, TurretParameters>() } 
            };


    }

    class BuildingComponentFactory<TComponentParameters> : IBuildingComponentFactory
    {
        private readonly BuildingComponent<TComponentParameters> parameters;
        private readonly ComponentFactory<Building, TComponentParameters> buildingFactory;
        private readonly ComponentFactory<BuildingGhost, TComponentParameters> ghostFactory;
        private readonly ComponentFactory<BuildingPlaceholder, TComponentParameters> placeholderFactory;

        public BuildingComponentFactory(BuildingComponent<TComponentParameters> parameters,
            ComponentFactory<Building, TComponentParameters> buildingFactory,
            ComponentFactory<BuildingGhost, TComponentParameters> ghostFactory,
            ComponentFactory<BuildingPlaceholder, TComponentParameters> placeholderFactory)
        {
            this.parameters = parameters;
            this.buildingFactory = buildingFactory;
            this.ghostFactory = ghostFactory;
            this.placeholderFactory = placeholderFactory;
        }

    }

    interface IBuildingComponentFactory
    {
        // create for ghost? yay nay
    }

    class ComponentFactory<TOwner, TComponentParameters> : IComponentFactory<TOwner>
    {
        private readonly TComponentParameters parameters;
        private readonly Func<TComponentParameters, IComponent<TOwner>> factory;

        public ComponentFactory(TComponentParameters parameters, Func<TComponentParameters, IComponent<TOwner>> factory)
        {
            this.parameters = parameters;
            this.factory = factory;
        }

        public IComponent<TOwner> Create() => factory(parameters);
    }

    interface IComponentFactory<TOwner>
    {
        IComponent<TOwner> Create();
    }
}
