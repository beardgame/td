namespace Bearded.TD.Networking.Loading
{
    class ClientLoadingManager : LoadingManager
    {
        public ClientLoadingManager(NetworkInterface networkInterface, IDataMessageHandler dataMessageHandler)
            : base(networkInterface, dataMessageHandler)
        {
        }
    }
}