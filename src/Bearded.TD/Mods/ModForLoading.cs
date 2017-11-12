using System;
using System.Threading.Tasks;

namespace Bearded.TD.Mods
{
    sealed class ModForLoading
    {
        private readonly ModMetadata modMetadata;
        private Mod mod;
        private bool isLoading;

        public bool IsLoaded => mod != null;

        public ModForLoading(ModMetadata modMetadata)
        {
            this.modMetadata = modMetadata;
        }

        public void StartLoading()
        {
            if (isLoading)
                throw new InvalidOperationException("Cannot load mod more than once.");

            isLoading = true;

            Task.Run(load);
        }

        private async Task load()
        {
            mod = await ModLoader.Load(modMetadata);
        }

        public Mod GetLoadedMod()
        {
            if (!IsLoaded)
                throw new InvalidOperationException("Must finish loading mod.");

            return mod;
        }
    }
}
