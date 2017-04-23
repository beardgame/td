namespace Bearded.TD.Networking.Lobby
{
    struct ClientInfo
    {
        public string PlayerName { get; }
        // API version
        // Hash of gamedata files

        public ClientInfo(string playerName)
        {
            PlayerName = playerName;
        }
    }
}