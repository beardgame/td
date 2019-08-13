using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game.Components;
using Bearded.Utilities.Linq;
using IComponent = Bearded.TD.Content.Serialization.Models.IComponent;

namespace Bearded.TD.Content.Models
{
    sealed class ComponentOwnerBlueprint : IComponentOwnerBlueprint
    {
        public string Id { get; }
        private readonly IReadOnlyCollection<IComponent> componentParameters;
        
        private readonly Dictionary<Type, object> componentFactoriesByOwnerType = new Dictionary<Type, object>();

        public IEnumerable<IComponent<T>> GetComponents<T>()
        {
            return getFactories<T>().Select(f => f.Create());
        }

        private IReadOnlyCollection<IComponentFactory<T>> getFactories<T>()
        {
            if (componentFactoriesByOwnerType.TryGetValue(typeof(T), out var factories))
                return (IReadOnlyCollection<IComponentFactory<T>>) factories;

            return createFactories<T>();
        }

        private IReadOnlyCollection<IComponentFactory<T>> createFactories<T>()
        {
            var factories = componentParameters.Select(ComponentFactories.CreateComponentFactory<T>).NotNull()
                .ToList().AsReadOnly();
            
            componentFactoriesByOwnerType.Add(typeof(T), factories);

            return factories;
        }

        public ComponentOwnerBlueprint(string id, IEnumerable<IComponent> components)
        {
            Id = id;

            componentParameters = (components?.ToList() ?? new List<IComponent>()).AsReadOnly();
        }
    }
}
