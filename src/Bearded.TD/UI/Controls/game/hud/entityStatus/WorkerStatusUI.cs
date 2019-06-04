using Bearded.TD.Game;
using Bearded.TD.Game.Factions;
using Bearded.UI.Navigation;

namespace Bearded.TD.UI.Controls
{
    sealed class WorkerStatusUI : NavigationNode<Faction>
    {
        public GameInstance Game { get; private set; }
        public Faction Faction { get; private set; }

        protected override void Initialize(DependencyResolver dependencies, Faction faction)
        {
            Game = dependencies.Resolve<GameInstance>();
            Faction = faction;
        }

        public void OnCloseClicked()
        {
            Navigation.Exit();
        }
    }
}
