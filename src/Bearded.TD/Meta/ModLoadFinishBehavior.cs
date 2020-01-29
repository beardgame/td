namespace Bearded.TD.Meta
{
    enum ModLoadFinishBehavior
    {
        // Go straight to the game after loading.
        DoNothing,
        // Propagate errors after loading. Go straight to the game after loading otherwise.
        ThrowOnError,
        // Wait for user input if there are errors after loading. Go straight to the game after loading otherwise.
        PauseOnError,
        // Always wait for user input after loading.
        AlwaysPause
    }
}
