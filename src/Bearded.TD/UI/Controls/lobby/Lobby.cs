using amulware.Graphics;
using Bearded.UI.Navigation;

namespace Bearded.TD.UI.Controls
{
    sealed class Lobby : NavigationNode<LobbyManager>
    {
        private LobbyManager lobbyManager;

        protected override void Initialize(DependencyResolver dependencies, LobbyManager lobbyManager)
        {
            this.lobbyManager = lobbyManager;
        }

        public void Update(UpdateEventArgs args)
        {
            lobbyManager.Update(args);
        }
    }
}
