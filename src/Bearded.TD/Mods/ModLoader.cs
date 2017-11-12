using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bearded.TD.Mods.Models;
using Bearded.TD.Mods.Serialization.Converters;
using Newtonsoft.Json;

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

                var footprints = loadFootprints();

                return new Mod(footprints);
            }

            private void configureSerializer()
            {
                serializer = new JsonSerializer();
                serializer.Converters.Add(new StepConverter());
            }

            private IEnumerable<FootprintGroup> loadFootprints()
            {
                const string path = "defs/footprints";

                var files = meta.Directory
                    .GetDirectories(path, SearchOption.TopDirectoryOnly)
                    .SingleOrDefault()
                    ?.GetFiles("*.json", SearchOption.TopDirectoryOnly);

                if (files == null)
                    return Enumerable.Empty<FootprintGroup>();

                var footPrints = new List<FootprintGroup>();

                foreach (var file in files)
                {
                    var text = file.OpenText();
                    var reader = new JsonTextReader(text);
                    var footPrint = serializer.Deserialize<Serialization.Models.FootprintGroup>(reader);

                    footPrints.Add(footPrint.ToGameModel());
                }

                return footPrints;
            }
        }
    }
}
