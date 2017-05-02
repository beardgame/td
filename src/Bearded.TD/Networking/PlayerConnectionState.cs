namespace Bearded.TD.Networking
{
    public enum PlayerConnectionState : byte
    {
        Unknown = 0,
        Connecting = 1,
        Waiting = 2,
        Ready = 3,
        AwaitingLoadingData = 4,
        ProcessingLoadingData = 5,
        FinishedLoading = 6,
        Playing = 7,
    }
}