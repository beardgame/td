using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Bearded.TD.Game;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Technologies;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.Utilities.Collections;
using TechnologyBlueprintJson = Bearded.TD.Content.Serialization.Models.TechnologyBlueprint;

namespace Bearded.TD.Content.Mods.BlueprintLoaders
{
    sealed class TechnologyBlueprintLoader
        : BaseBlueprintLoader<
            ITechnologyBlueprint, TechnologyBlueprintJson, TechnologyBlueprintJson.DependencyResolvers>
    {
        private readonly ReadonlyBlueprintCollection<IBuildingBlueprint> buildings;
        private readonly ReadonlyBlueprintCollection<IUpgradeBlueprint> upgrades;

        protected override string RelativePath => "defs/technologies";

        public TechnologyBlueprintLoader(BlueprintLoadingContext context,
            ReadonlyBlueprintCollection<IBuildingBlueprint> buildings,
            ReadonlyBlueprintCollection<IUpgradeBlueprint> upgrades) : base(context)
        {
            this.buildings = buildings;
            this.upgrades = upgrades;
        }

        protected override List<ITechnologyBlueprint> LoadBlueprintsFromFiles(FileInfo[] files)
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

            var buildingResolver = new BlueprintDependencyResolver<IBuildingBlueprint>(
                Context.Meta, buildings, Enumerable.Empty<Mod>(), m => m.Blueprints.Buildings);
            var upgradeResolver = new BlueprintDependencyResolver<IUpgradeBlueprint>(
                Context.Meta, upgrades, Enumerable.Empty<Mod>(), m => m.Blueprints.Upgrades);
            var technologyResolver = new AccumulatingBlueprintDependencyResolver<ITechnologyBlueprint>(
                Context.Meta, accumulatingBlueprintCollection, Enumerable.Empty<Mod>(), m => m.Blueprints.Technologies);

            var dependencyResolvers =
                new TechnologyBlueprintJson.DependencyResolvers(buildingResolver, upgradeResolver, technologyResolver);

            foreach (var jsonModel in sortedJsonModels)
            {
                ITechnologyBlueprint blueprint;
                try
                {
                    blueprint = jsonModel.ToGameModel(dependencyResolvers);
                }
                catch (InvalidDataException e)
                {
                    logLoadingException(e, jsonModel.Name);
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
            // TODO: https://en.wikipedia.org/wiki/Topological_sorting#Algorithms
            // basically depth-first it

            return models;
        }
    }
}
