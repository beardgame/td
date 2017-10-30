using System.Threading.Tasks;

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

            public Loader(ModMetadata meta)
            {
                this.meta = meta;
            }

            public async Task<Mod> Load()
            {
                // do loading here

                await Task.Delay(1000);

                return new Mod();
            }
        }
    }
}
