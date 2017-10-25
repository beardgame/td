using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bearded.TD.Networking.MasterServer
{
    class MasterServer
    {
        private readonly IMasterServerClient client;

        public MasterServer(IMasterServerClient client)
        {
            this.client = client;
        }

        public async Task<int> RegisterLobby(Proto.Lobby lobbyInfo)
        {
            var request = new Proto.RegisterLobbyRequest
            {
                GameInfo = gameInfo,
                Lobby = lobbyInfo
            };
            var result = await client.RegisterLobby(request);

            throwErrors(result.Error);
            return result.AssignedId;
        }

		public async void UnregisterLobby(int lobbyId)
		{
			var request = new Proto.UnregisterLobbyRequest
			{
				LobbyId = lobbyId
			};
            var result = await client.UnregisterLobby(request);

			throwErrors(result.Error);
		}

		public async Task<IList<Proto.Lobby>> GetLobbies()
		{
			var request = new Proto.ListLobbiesRequest
			{
				GameInfo = gameInfo
			};
			var result = await client.ListLobbies(request);

			throwErrors(result.Error);
			return result.Lobby;
		}

        public async void ConnectToLobby()
        {
            throw new NotImplementedException();
        }

        private Proto.GameInfo gameInfo => new Proto.GameInfo();

        private void throwErrors(IList<Proto.Error> error)
            => throw new MasterServerException(error[0].ErrorType, error[0].ErrorMessage);
    }
}
