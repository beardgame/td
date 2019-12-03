using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.TD.Game;
using Bearded.TD.Game.Technologies;
using Bearded.TD.Utilities.Collections;
using TechnologyBlueprintJson = Bearded.TD.Content.Serialization.Models.TechnologyBlueprint;

namespace Bearded.TD.Content.Mods.BlueprintLoaders
{
    sealed class TechnologyBlueprintLoader
        : BaseBlueprintLoader<ITechnologyBlueprint, TechnologyBlueprintJson, IDependencyResolver<ITechnologyBlueprint>>
    {
        protected override string RelativePath => "defs/technologies";

        public TechnologyBlueprintLoader(BlueprintLoadingContext context) : base(context)
        {
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
                // TODO: we should not be catching such a broad exception, but technology unlocks are trying to be smart
                //     and will immediately try to resolve a building or upgrade, and that might not exist. Resolving
                //     stuff like this should only be done on the JSON -> game conversion.
                catch (Exception e)
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
            var dependencyResolver = new AccumulatingBlueprintDependencyResolver<ITechnologyBlueprint>(
                Context.Meta, accumulatingBlueprintCollection, Enumerable.Empty<Mod>(), m => m.Blueprints.Technologies);

            foreach (var jsonModel in sortedJsonModels)
            {
                ITechnologyBlueprint blueprint;
                try
                {
                    blueprint = jsonModel.ToGameModel(dependencyResolver);
                }
                catch (Exception e)
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
