using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using amulware.Graphics.Serialization.JsonNet;
using Bearded.TD.Mods.Models;
using Bearded.TD.Mods.Serialization.Converters;
using Bearded.TD.Mods.Serialization.Models;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;
using Newtonsoft.Json;
using BuildingBlueprint = Bearded.TD.Mods.Models.BuildingBlueprint;
using BuildingBlueprintJson = Bearded.TD.Mods.Serialization.Models.BuildingBlueprint;
using FootprintGroup = Bearded.TD.Mods.Models.FootprintGroup;
using FootprintGroupJson = Bearded.TD.Mods.Serialization.Models.FootprintGroup;
using UnitBlueprint = Bearded.TD.Mods.Models.UnitBlueprint;
using UnitBlueprintJson = Bearded.TD.Mods.Serialization.Models.UnitBlueprint;
using WeaponBlueprint = Bearded.TD.Mods.Models.WeaponParameters;
using WeaponBlueprintJson = Bearded.TD.Mods.Serialization.Models.WeaponBlueprint;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.Mods
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
                return await Task.Run(load);
            }

            private async Task<Mod> load()
            {
                configureSerializer();

                var weapons = loadWeapons();

                configureSerializerDependency(weapons, m => m.Blueprints.Weapons);

                var footprints = loadFootprints();
                var buildings = loadBuildings(footprints);
                var units = loadUnits();

                return new Mod(
                    footprints,
                    buildings,
                    units,
                    weapons
                    );
            }

            private ReadonlyBlueprintCollection<FootprintGroup> loadFootprints()
                => loadBlueprints<FootprintGroup, FootprintGroupJson>("defs/footprints");

            private ReadonlyBlueprintCollection<BuildingBlueprint> loadBuildings(ReadonlyBlueprintCollection<FootprintGroup> footprints)
                => loadBlueprints<BuildingBlueprint, BuildingBlueprintJson, DependencyResolver<FootprintGroup>>(
                "defs/buildings",
                new DependencyResolver<FootprintGroup>(
                    meta, footprints, Enumerable.Empty<Mod>(),
                    m => m.Blueprints.Footprints
                    )
                );

            private ReadonlyBlueprintCollection<UnitBlueprint> loadUnits()
                => loadBlueprints<UnitBlueprint, UnitBlueprintJson>("defs/units");

            private ReadonlyBlueprintCollection<WeaponBlueprint> loadWeapons()
                => loadBlueprints<WeaponBlueprint, WeaponBlueprintJson>("defs/weapons");

            private static ReadonlyBlueprintCollection<T> empty<T>()
                where T : IBlueprint
                => new ReadonlyBlueprintCollection<T>(Enumerable.Empty<T>());

            private void configureSerializer()
            {
                serializer = new JsonSerializer();
                serializer.Converters.Add(new StepConverter());
                serializer.Converters.Add(new SpaceTime1Converter<Unit>(v => v.U()));
                serializer.Converters.Add(new SpaceTime1Converter<Speed>(v => v.UnitsPerSecond()));
                serializer.Converters.Add(new SpaceTime1Converter<Bearded.Utilities.SpaceTime.TimeSpan>(v => ((double) v).S()));
                serializer.Converters.Add(Converters.ColorContainerConverter);
                serializer.Converters.Add(BuildingComponentConverterFactory.Make());
            }

            private void configureSerializerDependency<TBlueprint>(
                    ReadonlyBlueprintCollection<TBlueprint> blueprints,
                    Func<Mod, ReadonlyBlueprintCollection<TBlueprint>> blueprintSelector)
                    where TBlueprint : IBlueprint
            {
                var dependencyResolver =
                        new DependencyResolver<TBlueprint>(meta, blueprints, Enumerable.Empty<Mod>(), blueprintSelector);
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

                        blueprints.Add(gameModel);
                    }
                    catch (Exception e)
                    {
                        context.Logger.Warning.Log($"Error loading '{meta.Id}/{path}/../{file.Name}': {e.Message}");
                    }
                }

                return blueprints;
            }

            private FileInfo[] jsonFilesIn(string path)
                => meta.Directory
                    .GetDirectories(path, SearchOption.TopDirectoryOnly)
                    .SingleOrDefault()
                    ?.GetFiles("*.json", SearchOption.AllDirectories);
        }
    }
}
