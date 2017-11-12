using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Mods.Models;
using Bearded.TD.Mods.Serialization.Converters;
using Bearded.TD.Mods.Serialization.Models;
using Bearded.Utilities;
using Newtonsoft.Json;
using BuildingBlueprint = Bearded.TD.Mods.Models.BuildingBlueprint;
using BuildingBlueprintJson = Bearded.TD.Mods.Serialization.Models.BuildingBlueprint;
using FootprintGroup = Bearded.TD.Mods.Models.FootprintGroup;
using FootprintGroupJson = Bearded.TD.Mods.Serialization.Models.FootprintGroup;

namespace Bearded.TD.Mods
{
    static class ModLoader
    {
        public static async Task<Mod> Load(ModMetadata mod)
        {
            return await new Loader(mod).Load();
        }

        private sealed class Loader
        {
            private readonly ModMetadata meta;
            private JsonSerializer serializer;

            public Loader(ModMetadata meta)
            {
                this.meta = meta;
            }

            public async Task<Mod> Load()
            {
                return await Task.Run(load);
            }

            private async Task<Mod> load()
            {
                configureSerializer();

                var footprints = loadBlueprints<FootprintGroup, FootprintGroupJson>("defs/footprints");
                var buildings =
                    loadBlueprints<BuildingBlueprint, BuildingBlueprintJson, DependencyResolver<FootprintGroup>>(
                        "defs/buildings",
                        new DependencyResolver<FootprintGroup>(meta, footprints, Enumerable.Empty<Mod>(),
                            m => m.Blueprints.Footprints)
                    );

                return new Mod(
                    footprints,
                    empty<ComponentFactory>(),
                    buildings,
                    empty<UnitBlueprint>()
                    );
            }

            private static ReadonlyBlueprintCollection<T> empty<T>()
                where T : IBlueprint
                => new ReadonlyBlueprintCollection<T>(Enumerable.Empty<T>());

            private void configureSerializer()
            {
                serializer = new JsonSerializer();
                serializer.Converters.Add(new StepConverter());
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
                var files = meta.Directory
                    .GetDirectories(path, SearchOption.TopDirectoryOnly)
                    .SingleOrDefault()
                    ?.GetFiles("*.json", SearchOption.AllDirectories);

                if (files == null)
                    return new ReadonlyBlueprintCollection<TBlueprint>(Enumerable.Empty<TBlueprint>());

                var blueprints = new List<TBlueprint>();

                foreach (var file in files)
                {
                    var text = file.OpenText();
                    var reader = new JsonTextReader(text);
                    var jsonModel = serializer.Deserialize<TJsonModel>(reader);

                    blueprints.Add(jsonModel.ToGameModel(resolvers));
                }

                return new ReadonlyBlueprintCollection<TBlueprint>(blueprints);
            }
        }
    }
}
