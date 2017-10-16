namespace Bearded.TD.Mods
{
    sealed class ModForLoading
    {
        private readonly ModMetadata modMetadata;
        private bool isLoaded;

        public ModForLoading(ModMetadata modMetadata)
        {
            this.modMetadata = modMetadata;
        }

        public void Load()
        {
            isLoaded = true;
        }

        public Mod AsMod()
        {
            if (!isLoaded)
                Load();
            return new Mod();
        }
    }
}
