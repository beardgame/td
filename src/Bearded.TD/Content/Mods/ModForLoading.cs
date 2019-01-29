using System;
using System.Threading.Tasks;

namespace Bearded.TD.Content.Mods
{
    sealed class ModForLoading
    {
        private readonly ModMetadata modMetadata;
        private Mod mod;
        private bool isLoading;
        private ModLoadingContext context;
        private Exception exception;

        public bool IsDone { get; private set; }
        public bool DidLoadSuccessfully => IsDone && exception != null;

        public ModForLoading(ModMetadata modMetadata)
        {
            this.modMetadata = modMetadata;
        }

        public void StartLoading(ModLoadingContext context)
        {
            if (isLoading)
                throw new InvalidOperationException("Cannot load mod more than once.");

            isLoading = true;
            this.context = context;

            Task.Run(load);
        }

        private async Task load()
        {
            try
            {
                mod = await ModLoader.Load(context, modMetadata);
            }
            catch (Exception e)
            {
                exception = e;
                context.Logger.Error?.Log($"Error loading mod {modMetadata.Id}: {e.Message}");
            }
            finally
            {
                IsDone = true;
            }
        }

        public Mod GetLoadedMod()
        {
            if (!IsDone)
                throw new InvalidOperationException("Must finish loading mod.");

            if (exception != null)
                throw new Exception($"Something went wrong loading mod '{modMetadata.Id}'", exception);

            return mod;
        }
    }
}
