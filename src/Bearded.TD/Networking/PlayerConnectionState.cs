namespace Bearded.TD.Networking
{
    public enum PlayerConnectionState : byte
    {
        Unknown = 0,
        Connecting = 1,
        Waiting = 2,
        Ready = 3,
        LoadingMods = 4,
        AwaitingLoadingData = 5,
        ProcessingLoadingData = 6,
        FinishedLoading = 7,
        Playing = 8,
    }
}