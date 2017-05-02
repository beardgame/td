namespace Bearded.TD.Networking.Loading
{
    class ServerLoadingManager : LoadingManager
    {
        public ServerLoadingManager(NetworkInterface networkInterface, IDataMessageHandler dataMessageHandler)
            : base(networkInterface, dataMessageHandler)
        {
        }
    }
}