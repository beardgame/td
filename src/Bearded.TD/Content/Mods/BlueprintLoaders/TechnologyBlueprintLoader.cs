using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.TD.Game;
using Bearded.TD.Game.Technologies;
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

        // TODO: error handling per file
        protected override List<ITechnologyBlueprint> LoadBlueprintsFromFiles(FileInfo[] files)
        {
            var jsonModels = loadJsonModels(files);
            var sortedJsonModels = topologicalSort(jsonModels);

            var blueprints = new List<ITechnologyBlueprint>();
            var accumulatingBlueprintCollection = new BlueprintCollection<ITechnologyBlueprint>();
            var dependencyResolver = new AccumulatingBlueprintDependencyResolver<ITechnologyBlueprint>(
                Context.Meta, accumulatingBlueprintCollection, Enumerable.Empty<Mod>(), m => m.Blueprints.Technologies);

            foreach (var jsonModel in sortedJsonModels)
            {
                var blueprint = jsonModel.ToGameModel(dependencyResolver);
                blueprints.Add(blueprint);
                accumulatingBlueprintCollection.Add(blueprint);
            }

            return blueprints;
        }

        private IEnumerable<TechnologyBlueprintJson> loadJsonModels(IEnumerable<FileInfo> files) =>
            files.Select(ParseJsonModel);

        private static IEnumerable<TechnologyBlueprintJson> topologicalSort(IEnumerable<TechnologyBlueprintJson> models)
        {
            // TODO: https://en.wikipedia.org/wiki/Topological_sorting#Algorithms
            // basically depth-first it

            return models;
        }
    }
}
