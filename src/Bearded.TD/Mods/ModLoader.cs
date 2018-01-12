using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bearded.TD.Game.Components;
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

                var footprints = loadFootprints();
                var buildings = loadBuildings(footprints);

                return new Mod(
                    footprints,
                    buildings,
                    empty<UnitBlueprint>()
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

            private static ReadonlyBlueprintCollection<T> empty<T>()
                where T : IBlueprint
                => new ReadonlyBlueprintCollection<T>(Enumerable.Empty<T>());

            private void configureSerializer()
            {
                serializer = new JsonSerializer();
                serializer.Converters.Add(new StepConverter());
                serializer.Converters.Add(new SpaceTime1Converter<Unit>(v => new Unit(v)));
                serializer.Converters.Add(BuildingComponentConverterFactory.Make());
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
