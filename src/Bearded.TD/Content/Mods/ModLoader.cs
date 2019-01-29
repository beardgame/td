using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
using Bearded.TD.Game.Projectiles;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.Game.Weapons;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using Newtonsoft.Json;
using BuildingBlueprintJson = Bearded.TD.Content.Serialization.Models.BuildingBlueprint;
using FootprintGroup = Bearded.TD.Game.World.FootprintGroup;
using FootprintGroupJson = Bearded.TD.Content.Serialization.Models.FootprintGroup;
using ProjectileBlueprintJson = Bearded.TD.Content.Serialization.Models.ProjectileBlueprint;
using SpriteSet = Bearded.TD.Content.Models.SpriteSet;
using UnitBlueprintJson = Bearded.TD.Content.Serialization.Models.UnitBlueprint;
using WeaponBlueprintJson = Bearded.TD.Content.Serialization.Models.WeaponBlueprint;
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

                var sprites = loadSprites();

                configureSpriteSerializerDependency(sprites);

                var projectiles = loadProjectiles();

                configureSerializerDependency(projectiles, m => m.Blueprints.Projectiles);

                var weapons = loadWeapons();

                configureSerializerDependency(weapons, m => m.Blueprints.Weapons);

                var footprints = loadFootprints();
                var buildings = loadBuildings(footprints, tags);
                var units = loadUnits();

                return new Mod(
                    meta.Id,
                    meta.Name,
                    sprites,
                    footprints,
                    buildings,
                    units,
                    weapons,
                    projectiles,
                    ImmutableDictionary<Id<UpgradeBlueprint>, UpgradeBlueprint>.Empty,
                    tags.GetForCurrentMod());
            }
            
            private ReadonlyBlueprintCollection<SpriteSet> loadSprites()
            {
                var spriteSetFiles = jsonFilesIn("gfx/sprites");
                var spriteSets = loadSpriteSets(spriteSetFiles);
                return new ReadonlyBlueprintCollection<SpriteSet>(spriteSets);
            }

            private IEnumerable<SpriteSet> loadSpriteSets(IEnumerable<FileInfo> spriteSetFiles)
            {
                var loader = new SpriteSetLoader(context, meta, serializer);
                return spriteSetFiles.Select(loader.TryLoad).Where(s => s != null);
            }
            
            private ReadonlyBlueprintCollection<IBuildingBlueprint> loadBuildings(ReadonlyBlueprintCollection<FootprintGroup> footprints, UpgradeTagResolver tagResolver)
                => loadBlueprints<IBuildingBlueprint, BuildingBlueprintJson, (DependencyResolver<FootprintGroup> footprints, UpgradeTagResolver tags)>(
                "defs/buildings",
                (
                    new DependencyResolver<FootprintGroup>(
                        meta, footprints, Enumerable.Empty<Mod>(),
                        m => m.Blueprints.Footprints
                        ),
                    tagResolver
                ));

            private ReadonlyBlueprintCollection<FootprintGroup> loadFootprints()
                => loadBlueprints<FootprintGroup, FootprintGroupJson>("defs/footprints");

            private ReadonlyBlueprintCollection<IProjectileBlueprint> loadProjectiles()
                => loadBlueprints<IProjectileBlueprint, ProjectileBlueprintJson>("defs/projectiles");

            private ReadonlyBlueprintCollection<IUnitBlueprint> loadUnits()
                => loadBlueprints<IUnitBlueprint, UnitBlueprintJson>("defs/units");

            private ReadonlyBlueprintCollection<IWeaponBlueprint> loadWeapons()
                => loadBlueprints<IWeaponBlueprint, WeaponBlueprintJson>("defs/weapons");

            private void configureSerializer()
            {
                serializer = new JsonSerializer();
                serializer.Converters.Add(new StepConverter());
                serializer.Converters.Add(new SpaceTime1Converter<Unit>(v => v.U()));
                serializer.Converters.Add(new SpaceTime1Converter<Speed>(v => v.UnitsPerSecond()));
                serializer.Converters.Add(new SpaceTime1Converter<Bearded.Utilities.SpaceTime.TimeSpan>(v => ((double) v).S()));
                serializer.Converters.Add(Converters.ColorContainerConverter);
                serializer.Converters.Add(ComponentConverterFactory.ForBuildingComponents());
                serializer.Converters.Add(ComponentConverterFactory.ForBaseComponent());
                foreach (var entry in ParametersTemplateLibrary.Instance.GetInterfaceToTemplateMap())
                    serializer.Converters.Add(new TechEffectTemplateConverter(entry.Key, entry.Value));
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
                var dependencyResolver = new DependencyResolver<TBlueprint>(
                    meta, blueprints, Enumerable.Empty<Mod>(), blueprintSelector);
                serializer.Converters.Add(new DependencyConverter<TBlueprint>(dependencyResolver));
            }

            private ReadonlyBlueprintCollection<TBlueprint> loadBlueprints
                <TBlueprint, TJsonModel>(string path)
                where TBlueprint : IBlueprint
                where TJsonModel : IConvertsTo<TBlueprint, Void>
                => loadBlueprints<TBlueprint, TJsonModel, Void>(path, default(Void));

            private ReadonlyBlueprintCollection<TBlueprint> loadBlueprints
                <TBlueprint, TJsonModel, TResolvers>(string path, TResolvers resolvers)
                where TBlueprint : IBlueprint
                where TJsonModel : IConvertsTo<TBlueprint, TResolvers>
            {
                var files = jsonFilesIn(path);

                return new ReadonlyBlueprintCollection<TBlueprint>(
                    files.IsNullOrEmpty()
                        ? Enumerable.Empty<TBlueprint>()
                        : loadBlueprintsFromFiles<TBlueprint, TJsonModel, TResolvers>(path, resolvers, files)
                    );
            }

            private List<TBlueprint> loadBlueprintsFromFiles
                <TBlueprint, TJsonModel, TResolvers>(string path, TResolvers resolvers, FileInfo[] files)
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
                        var gameModel = jsonModel.ToGameModel(resolvers);

                        if (Path.GetFileNameWithoutExtension(file.FullName) != gameModel.Id)
                        {
                            context.Logger.Warning?.Log(
                                    $"Loaded blueprint {meta.Id}.{gameModel.Id} with mismatching filename {file.Name}");
                        }

                        blueprints.Add(gameModel);
                    }
                    catch (Exception e)
                    {
                        context.Logger.Error?.Log($"Error loading '{meta.Id}/{path}/../{file.Name}': {e.Message}");
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
