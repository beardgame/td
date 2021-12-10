using Lidgren.Network;

namespace Bearded.TD.Networking;

interface INetworkMessageHandler
{
    bool Accepts(NetIncomingMessage message);

    void Handle(NetIncomingMessage message);
}