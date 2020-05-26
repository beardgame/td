namespace Bearded.TD.Game.Players
{
    public enum PlayerConnectionState : byte
    {
        Unknown = 0,
        Connecting = 1,
        Waiting = 2,
        LoadingMods = 3,
        Ready = 4,
        AwaitingLoadingData = 5,
        ProcessingLoadingData = 6,
        FinishedLoading = 7,
        Playing = 8,
    }
}
