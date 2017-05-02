namespace Bearded.TD.Networking.Lobby
{
    public enum PlayerConnectionState : byte
    {
        Unknown = 0,
        Connecting = 1,
        Waiting = 2,
        Ready = 3,
        Loading = 4,
        FinishedLoading = 5,
        Playing = 6,
    }
}