using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Serialization.Converters;
using Bearded.TD.Game;
using Bearded.TD.Game.Components;
using Bearded.Utilities;

namespace Bearded.TD.Content.Mods.BlueprintLoaders
{
    class ComponentOwnerBlueprintLoader : BaseBlueprintLoader<IComponentOwnerBlueprint, Serialization.Models.ComponentOwnerBlueprint, Void>
    {
        private readonly ComponentOwnerProxyBlueprintResolver componentOwnerProxyBlueprintCollector;
        private readonly DependencyConverter<IComponentOwnerBlueprint> dependencyConverter;
        private readonly Dictionary<string, (ComponentOwnerBlueprint Blueprint, FileInfo File, List<ComponentOwnerBlueprintProxy> Dependencies)>
            dependenciesByBlueprintId = new Dictionary<string, (ComponentOwnerBlueprint, FileInfo, List<ComponentOwnerBlueprintProxy>)>();

        protected override string RelativePath => "defs/blueprints";

        protected override DependencySelector SelectDependency => m => m.Blueprints.ComponentOwners;

        public ComponentOwnerBlueprintLoader(BlueprintLoadingContext context) : base(context)
        {
            componentOwnerProxyBlueprintCollector = new ComponentOwnerProxyBlueprintResolver(Context.Meta, Enumerable.Empty<Mod>());
            dependencyConverter = new DependencyConverter<IComponentOwnerBlueprint>(componentOwnerProxyBlueprintCollector);
        }

        public override ReadonlyBlueprintCollection<IComponentOwnerBlueprint> LoadBlueprints()
        {
            var blueprints = loadBlueprints();

            var validBlueprints = blueprints.Where(resolveDependencies);

            var blueprintCollection = new ReadonlyBlueprintCollection<IComponentOwnerBlueprint>(validBlueprints);

            SetupDependencyResolver(blueprintCollection);

            return blueprintCollection;
        }

        private List<IComponentOwnerBlueprint> loadBlueprints()
        {
            Context.Serializer.Converters.Add(dependencyConverter);

            var files = GetJsonFiles();

            var blueprints = LoadBlueprintsFromFiles(files);

            Context.Serializer.Converters.Remove(dependencyConverter);

            return blueprints;
        }

        private bool resolveDependencies(IComponentOwnerBlueprint blueprint)
        {
            var (_, file, proxies) = dependenciesByBlueprintId[blueprint.Id];

            foreach (var proxy in proxies)
            {
                if (!dependenciesByBlueprintId.TryGetValue(proxy.Id, out var dependency))
                {
                    LogError($"Error loading '{Context.Meta.Id}/{RelativePath}/../{file.Name}': Could not find reference '{proxy.Id}'");

                    return false;
                }

                proxy.InjectActualBlueprint(dependency.Blueprint);
            }

            return true;
        }

        protected override IComponentOwnerBlueprint LoadBlueprint(FileInfo file)
        {
            var blueprint = base.LoadBlueprint(file);

            var proxies = componentOwnerProxyBlueprintCollector.GetAndResetCurrentProxies();

            dependenciesByBlueprintId[blueprint.Id] = ((ComponentOwnerBlueprint) blueprint, file, proxies);

            return blueprint;
        }
    }
}
