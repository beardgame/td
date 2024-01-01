using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Bearded.TD.Utilities;

namespace Bearded.TD.Content.Mods;

sealed class ModForLoading(ModMetadata modMetadata)
{
    private Mod? mod;
    private bool isLoading;
    private ModLoadingContext? context;
    private Exception? exception;
    private ReadOnlyCollection<Mod>? loadedDependencies;

    public bool IsDone { get; private set; }
    public bool DidLoadSuccessfully => IsDone && exception == null;

    public void StartLoading(ModLoadingContext context, ReadOnlyCollection<Mod> loadedDependencies)
    {
        if (isLoading)
            throw new InvalidOperationException("Cannot load mod more than once.");

        isLoading = true;
        this.context = context;
        this.loadedDependencies = loadedDependencies;

        Task.Run(load);
    }

    private async Task load()
    {
        try
        {
            mod = await ModLoader.Load(context!, modMetadata, loadedDependencies!);
        }
        catch (Exception e)
        {
            exception = e;
            context!.Logger.Error?.Log($"Error loading mod {modMetadata.Id}: {e.Message}");
        }
        finally
        {
            IsDone = true;
        }
    }

    public Mod GetLoadedMod()
    {
        if (!IsDone)
        {
            throw new InvalidOperationException("Must finish loading mod.");
        }
        if (exception != null)
        {
            throw new Exception($"Something went wrong loading mod '{modMetadata.Id}'", exception);
        }

        return mod!;
    }

    public void Rethrow()
    {
        DebugAssert.State.Satisfies(exception != null);
        throw exception!;
    }
}
