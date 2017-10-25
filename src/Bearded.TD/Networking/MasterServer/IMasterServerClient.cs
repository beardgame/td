using System.Threading.Tasks;
using Bearded.TD.Proto;

namespace Bearded.TD.Networking.MasterServer
{
    interface IMasterServerClient
    {
        bool IsConnected { get; }

        Task<RegisterLobbyResponse> RegisterLobby(RegisterLobbyRequest request);
        Task<UnregisterLobbyResponse> UnregisterLobby(UnregisterLobbyRequest request);
        Task<ListLobbiesResponse> ListLobbies(ListLobbiesRequest request);
        Task<ConnectToLobbyResponse> ConnectToLobby(ConnectToLobbyRequest request);

        void Update();
    }
}
