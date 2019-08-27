using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using amulware.Graphics.Serialization.JsonNet;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Serialization.Converters;
using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Technologies;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using Newtonsoft.Json;
using BuildingBlueprintJson = Bearded.TD.Content.Serialization.Models.BuildingBlueprint;
using ComponentOwnerBlueprint = Bearded.TD.Content.Models.ComponentOwnerBlueprint;
using FootprintGroup = Bearded.TD.Game.World.FootprintGroup;
using FootprintGroupJson = Bearded.TD.Content.Serialization.Models.FootprintGroup;
using Material = Bearded.TD.Content.Models.Material;
using MaterialJson = Bearded.TD.Content.Serialization.Models.Material;
using SpriteSet = Bearded.TD.Content.Models.SpriteSet;
using SpriteSetJson = Bearded.TD.Content.Serialization.Models.SpriteSet;
using Shader = Bearded.TD.Content.Models.Shader;
using ShaderJson = Bearded.TD.Content.Serialization.Models.Shader;
using TechnologyBlueprintJson = Bearded.TD.Content.Serialization.Models.TechnologyBlueprint;
using UnitBlueprintJson = Bearded.TD.Content.Serialization.Models.UnitBlueprint;
using UpgradeBlueprintJson = Bearded.TD.Content.Serialization.Models.UpgradeBlueprint;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.Content.Mods
{
    static class ModLoader
    {
        public static async Task<Mod> Load(ModLoadingContext context, ModMetadata mod)
        {
            return await new Loader(context, mod).Load();
        }

        private sealed class Loader
        {
            private readonly ModLoadingContext context;
            private readonly ModMetadata meta;
            private JsonSerializer serializer;

            public Loader(ModLoadingContext context, ModMetadata meta)
            {
                this.context = context;
                this.meta = meta;
            }

            public async Task<Mod> Load()
            {
                return await Task.Run(() => load());
            }

            private Mod load()
            {
                var tags = new UpgradeTagResolver(meta, Enumerable.Empty<Mod>());

                configureSerializer();

                var shaders = loadShaders();

                configureSerializerDependency(shaders, m => m.Blueprints.Shaders);

                var materials = loadMaterials();

                var sprites = loadSprites();

                configureSpriteSerializerDependency(sprites);
                
                var componentOwners = loadComponentOwners();

                configureSerializerDependency(componentOwners, m => m.Blueprints.ComponentOwners);

                var footprints = loadFootprints();
                var buildings = loadBuildings(footprints, tags);

                configureSerializerDependency(buildings, m => m.Blueprints.Buildings);

                var units = loadUnits();

                var upgrades = loadUpgrades();

                configureSerializerDependency(upgrades, m => m.Blueprints.Upgrades);

                var technologies = loadTechnologies();

                return new Mod(
                    meta.Id,
                    meta.Name,
                    shaders,
                    materials,
                    sprites,
                    footprints,
                    buildings,
                    units,
                    componentOwners,
                    upgrades,
                    technologies,
                    tags.GetForCurrentMod());
            }

            private ReadonlyBlueprintCollection<Shader> loadShaders()
            {
                var loader = new ShaderLoader(context, meta);
                return loadBlueprintsDependingOnJsonFile<Shader, ShaderJson, ShaderLoader>("gfx/shaders", loader);
            }

            private ReadonlyBlueprintCollection<Material> loadMaterials()
            {
                var loader = new MaterialLoader(context);
                return loadBlueprintsDependingOnJsonFile<Material, MaterialJson, MaterialLoader>("gfx/materials", loader);
            }

            private ReadonlyBlueprintCollection<SpriteSet> loadSprites()
            {
                var loader = new SpriteSetLoader(context);
                return loadBlueprintsDependingOnJsonFile<SpriteSet, SpriteSetJson, SpriteSetLoader>("gfx/sprites", loader);
            }

            private ReadonlyBlueprintCollection<IBuildingBlueprint> loadBuildings(ReadonlyBlueprintCollection<FootprintGroup> footprints, UpgradeTagResolver tagResolver)
                => loadBlueprints<IBuildingBlueprint, BuildingBlueprintJson, (BlueprintDependencyResolver<FootprintGroup> footprints, UpgradeTagResolver tags)>(
                "defs/buildings",
                (
                    new BlueprintDependencyResolver<FootprintGroup>(
                        meta, footprints, Enumerable.Empty<Mod>(),
                        m => m.Blueprints.Footprints
                        ),
                    tagResolver
                ));

            private ReadonlyBlueprintCollection<FootprintGroup> loadFootprints()
                => loadBlueprints<FootprintGroup, FootprintGroupJson>("defs/footprints");

            private ReadonlyBlueprintCollection<IUnitBlueprint> loadUnits()
                => loadBlueprints<IUnitBlueprint, UnitBlueprintJson>("defs/units");

            private ReadonlyBlueprintCollection<IComponentOwnerBlueprint> loadComponentOwners()
            {
                var componentOwnerProxyBlueprintCollector = new ComponentOwnerProxyBlueprintResolver(meta, Enumerable.Empty<Mod>());
                var dependencyConverter = new DependencyConverter<IComponentOwnerBlueprint>(componentOwnerProxyBlueprintCollector);
                
                var dependenciesByBlueprintId = new Dictionary<string, (ComponentOwnerBlueprint Blueprint, FileInfo File, List<ComponentOwnerBlueprintProxy> Dependencies)>();
                
                serializer.Converters.Add(dependencyConverter);

                var path = "defs/blueprints";
                
                var files = jsonFilesIn(path);
                
                if (files.IsNullOrEmpty())
                    return new ReadonlyBlueprintCollection<IComponentOwnerBlueprint>(Enumerable.Empty<IComponentOwnerBlueprint>());

                var blueprints = loadBlueprintsFromFiles<ComponentOwnerBlueprint, Serialization.Models.ComponentOwnerBlueprint, Void>
                        (path, files, postDeserializationAction: recordBlueprintDependencies)
                        .ToList();
                
                serializer.Converters.Remove(dependencyConverter);
                
                var validBlueprints = blueprints.Where(resolveDependencies);

                return new ReadonlyBlueprintCollection<IComponentOwnerBlueprint>(validBlueprints);

                void recordBlueprintDependencies(FileInfo file, ComponentOwnerBlueprint blueprint)
                {
                    var proxies = componentOwnerProxyBlueprintCollector.GetAndResetCurrentProxies();

                    dependenciesByBlueprintId[blueprint.Id] = (blueprint, file, proxies);
                }

                bool resolveDependencies(IComponentOwnerBlueprint blueprint)
                {
                    var (_, file, proxies) = dependenciesByBlueprintId[blueprint.Id];

                    foreach (var proxy in proxies)
                    {
                        if (!dependenciesByBlueprintId.TryGetValue(proxy.Id, out var dependency))
                        {
                            context.Logger.Error?.Log($"Error loading '{meta.Id}/{path}/../{file.Name}': Could not find reference '{proxy.Id}'");
                            
                            return false;
                        }
                        
                        proxy.InjectActualBlueprint(dependency.Blueprint);
                    }

                    return true;
                }
            }

            private ReadonlyBlueprintCollection<IUpgradeBlueprint> loadUpgrades()
                => loadBlueprints<IUpgradeBlueprint, UpgradeBlueprintJson>("defs/upgrades");

            private ReadonlyBlueprintCollection<ITechnologyBlueprint> loadTechnologies()
                => loadBlueprints<ITechnologyBlueprint, TechnologyBlueprintJson>("defs/technologies");

            private void configureSerializer()
            {
                serializer = new JsonSerializer();
                serializer.Converters.Add(new StepConverter());
                serializer.Converters.Add(new SpaceTime1Converter<Unit>(v => v.U()));
                serializer.Converters.Add(new SpaceTime1Converter<Speed>(SpaceTimeExtensions.UnitsPerSecond));
                serializer.Converters.Add(new SpaceTime1Converter<Bearded.Utilities.SpaceTime.TimeSpan>(v => ((double) v).S()));
                serializer.Converters.Add(new SpaceTime1Converter<Frequency>(v => ((double) v).PerSecond()));
                serializer.Converters.Add(new SpaceTime1Converter<Direction2>(Direction2.FromDegrees));
                serializer.Converters.Add(new SpaceTime1Converter<Angle>(Angle.FromDegrees));
                serializer.Converters.Add(new SpaceTime1Converter<AngularAcceleration>(AngularAcceleration.FromDegrees));
                serializer.Converters.Add(new SpaceTime1Converter<Volume>(v => new Volume(v)));
                serializer.Converters.Add(new SpaceTime1Converter<FlowRate>(r => new FlowRate(r)));
                serializer.Converters.Add(new SpaceTime1Converter<AngularAcceleration>(AngularAcceleration.FromDegrees));
                serializer.Converters.Add(new SpaceTime2Converter<Difference2>((x, y) => new Difference2(x, y)));
                serializer.Converters.Add(Converters.ColorContainerConverter);
                serializer.Converters.Add(ComponentConverterFactory.ForBuildingComponents());
                serializer.Converters.Add(ComponentConverterFactory.ForBaseComponent());
                serializer.Converters.Add(new TechnologyUnlockConverter());
                serializer.Converters.Add(new UpgradeEffectConverter());
                foreach (var (key, value) in ParametersTemplateLibrary.Instance.GetInterfaceToTemplateMap())
                    serializer.Converters.Add(new TechEffectTemplateConverter(key, value));
            }

            private void configureSpriteSerializerDependency(
                ReadonlyBlueprintCollection<SpriteSet> spriteSets)
            {
                var dependencyResolver = new SpriteResolver(meta, spriteSets, Enumerable.Empty<Mod>());
                serializer.Converters.Add(new DependencyConverter<ISprite>(dependencyResolver));
            }

            private void configureSerializerDependency<TBlueprint>(
                    ReadonlyBlueprintCollection<TBlueprint> blueprints,
                    Func<Mod, ReadonlyBlueprintCollection<TBlueprint>> blueprintSelector)
                    where TBlueprint : IBlueprint
            {
                var dependencyResolver = new BlueprintDependencyResolver<TBlueprint>(
                    meta, blueprints, Enumerable.Empty<Mod>(), blueprintSelector);
                serializer.Converters.Add(new DependencyConverter<TBlueprint>(dependencyResolver));
            }

            private ReadonlyBlueprintCollection<TBlueprint> loadBlueprintsDependingOnJsonFile
                <TBlueprint, TJsonModel, TResolvers>(string path, TResolvers resolvers)
                where TBlueprint : IBlueprint
                where TJsonModel : IConvertsTo<TBlueprint, (FileInfo, TResolvers)>
                => loadBlueprints<TBlueprint, TJsonModel, (FileInfo, TResolvers)>(path, file => (file, resolvers));

            private ReadonlyBlueprintCollection<TBlueprint> loadBlueprints
                <TBlueprint, TJsonModel>(string path)
                where TBlueprint : IBlueprint
                where TJsonModel : IConvertsTo<TBlueprint, Void>
                => loadBlueprints<TBlueprint, TJsonModel, Void>(path, default(Void));

            private ReadonlyBlueprintCollection<TBlueprint> loadBlueprints
                <TBlueprint, TJsonModel, TResolvers>(string path, TResolvers resolvers)
                where TBlueprint : IBlueprint
                where TJsonModel : IConvertsTo<TBlueprint, TResolvers>
                => loadBlueprints<TBlueprint, TJsonModel, TResolvers>(path, _ => resolvers);

            private ReadonlyBlueprintCollection<TBlueprint> loadBlueprints
                <TBlueprint, TJsonModel, TResolvers>(string path, Func<FileInfo, TResolvers> buildResolvers)
                where TBlueprint : IBlueprint
                where TJsonModel : IConvertsTo<TBlueprint, TResolvers>
            {
                var files = jsonFilesIn(path);

                return new ReadonlyBlueprintCollection<TBlueprint>(
                    files.IsNullOrEmpty()
                        ? Enumerable.Empty<TBlueprint>()
                        : loadBlueprintsFromFiles<TBlueprint, TJsonModel, TResolvers>(path, files, buildResolvers)
                    );
            }

            private List<TBlueprint> loadBlueprintsFromFiles<TBlueprint, TJsonModel, TResolvers>
                (string path, FileInfo[] files, Func<FileInfo, TResolvers> buildResolvers = null, 
                    Action<FileInfo, TBlueprint> postDeserializationAction = null)
                where TBlueprint : IBlueprint
                where TJsonModel : IConvertsTo<TBlueprint, TResolvers>
            {
                var blueprints = new List<TBlueprint>();

                foreach (var file in files)
                {
                    try
                    {
                        var text = file.OpenText();
                        var reader = new JsonTextReader(text);
                        var jsonModel = serializer.Deserialize<TJsonModel>(reader);
                        var finalResolvers = buildResolvers == null ? default(TResolvers) : buildResolvers(file);
                        var gameModel = jsonModel.ToGameModel(finalResolvers);

                        if (Path.GetFileNameWithoutExtension(file.FullName) != gameModel.Id)
                        {
                            context.Logger.Warning?.Log(
                                    $"Loaded blueprint {meta.Id}.{gameModel.Id} with mismatching filename {file.Name}");
                        }

                        postDeserializationAction?.Invoke(file, gameModel);

                        blueprints.Add(gameModel);
                    }
                    catch (Exception e)
                    {
                        context.Logger.Error?.Log($"Error loading '{meta.Id}/{path}/../{file.Name}': {e.Message}");
                        context.Logger.Debug?.Log(e.StackTrace);
                    }
                }

                return blueprints;
            }

            private FileInfo[] jsonFilesIn(string path)
            {
                var totalPath = Path.Combine(meta.Directory.FullName, path);

                if (!Directory.Exists(totalPath))
                    return new FileInfo[0];

                return meta
                    .Directory
                    .GetDirectories(path, SearchOption.TopDirectoryOnly)
                    .SingleOrDefault()
                    ?.GetFiles("*.json", SearchOption.AllDirectories)
                    ?? new FileInfo[0];
            }
        }
    }
}
