using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using Bearded.TD.Content.Serialization.Converters;

namespace Bearded.TD.Content.Mods.BlueprintLoaders
{
    sealed class BlueprintLoadingContext
    {
        public ModLoadingContext Context { get; }
        public ModMetadata Meta { get; }
        public JsonSerializerOptions SerializerOptions { get; }
        public ReadOnlyCollection<Mod> LoadedDependencies { get; }

        private readonly Dictionary<Type, object> dependencyResolvers = new();
        private readonly DependencyConverterFactory dependencyConverterFactory;

        public BlueprintLoadingContext(
            ModLoadingContext context,
            ModMetadata meta,
            JsonSerializerOptions serializerOptions,
            DependencyConverterFactory dependencyConverterFactory,
            ReadOnlyCollection<Mod> loadedDependencies)
        {
            Context = context;
            Meta = meta;
            SerializerOptions = serializerOptions;
            this.dependencyConverterFactory = dependencyConverterFactory;
            LoadedDependencies = loadedDependencies;
        }

        public void AddDependencyResolver<T>(IDependencyResolver<T> dependencyResolver)
        {
            dependencyResolvers[typeof(T)] = dependencyResolver;
            dependencyConverterFactory.RegisterDependencyResolver(dependencyResolver);
        }

        public void RemoveDependencyResolver<T>()
        {
            dependencyResolvers.Remove(typeof(T));
            dependencyConverterFactory.UnregisterDependencyResolver<T>();
        }

        public IDependencyResolver<T> FindDependencyResolver<T>() =>
            dependencyResolvers[typeof(T)] as IDependencyResolver<T>;
    }
}
