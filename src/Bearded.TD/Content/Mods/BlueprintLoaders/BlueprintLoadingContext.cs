using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Bearded.TD.Content.Serialization.Converters;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Mods.BlueprintLoaders;

sealed class BlueprintLoadingContext(
    ModLoadingContext context,
    ModMetadata meta,
    JsonSerializer serializer,
    ReadOnlyCollection<Mod> loadedDependencies)
{
    public ModLoadingContext Context { get; } = context;
    public ModMetadata Meta { get; } = meta;
    public JsonSerializer Serializer { get; } = serializer;
    public ReadOnlyCollection<Mod> LoadedDependencies { get; } = loadedDependencies;

    private readonly Dictionary<Type, object> dependencyResolvers = new();

    public void AddDependencyResolver<T>(IDependencyResolver<T> dependencyResolver)
    {
        dependencyResolvers[typeof(T)] = dependencyResolver;
        Serializer.Converters.Add(new DependencyConverter<T>(dependencyResolver));
    }

    public IDependencyResolver<T> FindDependencyResolver<T>() =>
        (IDependencyResolver<T>)dependencyResolvers[typeof(T)];
}
