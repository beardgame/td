using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using TechnologyBlueprintJson = Bearded.TD.Content.Serialization.Models.TechnologyBlueprint;

namespace Bearded.TD.Content.Mods.BlueprintLoaders;

sealed class TechnologyBlueprintLoader(
    BlueprintLoadingContext context,
    ReadonlyBlueprintCollection<IGameObjectBlueprint> gameObjects,
    ReadonlyBlueprintCollection<IPermanentUpgrade> upgrades)
    : BaseBlueprintLoader<
        ITechnologyBlueprint, TechnologyBlueprintJson, TechnologyBlueprintJson.DependencyResolvers>(context)
{
    protected override string RelativePath => "defs/technologies";
    protected override DependencySelector SelectDependency => m => m.Blueprints.Technologies;

    protected override List<ITechnologyBlueprint> LoadBlueprintsFromFiles(IEnumerable<FileInfo> files)
    {
        var jsonModels = loadJsonModels(files);
        var sortedJsonModels = topologicalSort(jsonModels);
        var blueprints = loadGameModels(sortedJsonModels);

        return blueprints;
    }

    private IEnumerable<TechnologyBlueprintJson> loadJsonModels(IEnumerable<FileInfo> files)
    {
        var jsonModels = new List<TechnologyBlueprintJson>();

        foreach (var file in files)
        {
            try
            {
                jsonModels.Add(ParseJsonModel(file));
            }
            catch (SerializationException e)
            {
                logLoadingException(e, file.Name);
            }
        }

        return jsonModels.AsReadOnlyEnumerable();
    }

    private List<ITechnologyBlueprint> loadGameModels(IEnumerable<TechnologyBlueprintJson> sortedJsonModels)
    {
        var blueprints = new List<ITechnologyBlueprint>();
        var accumulatingBlueprintCollection = new BlueprintCollection<ITechnologyBlueprint>();

        var gameObjectResolver = new BlueprintDependencyResolver<IGameObjectBlueprint>(
            Context.Meta, gameObjects, Context.LoadedDependencies, m => m.Blueprints.GameObjects);
        var upgradeResolver = new BlueprintDependencyResolver<IPermanentUpgrade>(
            Context.Meta, upgrades, Context.LoadedDependencies, m => m.Blueprints.Upgrades);
        var technologyResolver = new AccumulatingBlueprintDependencyResolver<ITechnologyBlueprint>(
            Context.Meta, accumulatingBlueprintCollection, Context.LoadedDependencies, m => m.Blueprints.Technologies);

        var dependencyResolvers =
            new TechnologyBlueprintJson.DependencyResolvers(gameObjectResolver, upgradeResolver, technologyResolver);

        foreach (var jsonModel in sortedJsonModels)
        {
            ITechnologyBlueprint blueprint;
            try
            {
                blueprint = jsonModel.ToGameModel(Context.Meta, dependencyResolvers);
            }
            catch (InvalidDataException e)
            {
                logLoadingException(e, jsonModel.Name ?? "UNKNOWN");
                continue;
            }

            blueprints.Add(blueprint);
            accumulatingBlueprintCollection.Add(blueprint);
        }

        return blueprints;
    }

    private void logLoadingException(Exception e, string name)
    {
        LogException(e, $"Error loading '{Context.Meta.Id}/{RelativePath}/../{name}': {e.Message}");
    }

    private static IEnumerable<TechnologyBlueprintJson> topologicalSort(IEnumerable<TechnologyBlueprintJson> models)
    {
        var modelsList = models.ToList();
        var unvisitedModelsByName = modelsList.ToDictionary(model => model.Id);
        var sortedModels = new List<TechnologyBlueprintJson>();

        while (unvisitedModelsByName.Count > 0)
        {
            topologicalSortVisit(unvisitedModelsByName.Values.First(), unvisitedModelsByName, sortedModels);
        }

        DebugAssert.State.Satisfies(() => modelsList.Count == sortedModels.Count);

        return sortedModels;
    }

    private static void topologicalSortVisit(
        TechnologyBlueprintJson model,
        IDictionary<string, TechnologyBlueprintJson> unvisitedModelsByName,
        ICollection<TechnologyBlueprintJson> sortedModels)
    {
        _ = model.Id ?? throw new InvalidDataException($"{nameof(model.Id)} must not be null");

        unvisitedModelsByName.Remove(model.Id);

        if (model.RequiredTechs != null)
        {
            foreach (var requiredName in model.RequiredTechs)
            {
                if (unvisitedModelsByName.TryGetValue(requiredName, out var requiredModel))
                {
                    topologicalSortVisit(requiredModel, unvisitedModelsByName, sortedModels);
                }
            }
        }

        sortedModels.Add(model);
    }
}
