using Bearded.TD.Game;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    sealed class GameStatusUI
    {
        public event VoidEventHandler StatusChanged;
        public event VoidEventHandler TechnologyButtonClicked;

        private GameInstance game;

        public long FactionResources => game.Me.Faction.Resources.CurrentResources;
        public int FactionResourceIncome => game.Me.Faction.Resources.CurrentIncome;
        public long FactionTechPoints => game.Me.Faction.Technology.TechPoints;

        public void Initialize(GameInstance game)
        {
            this.game = game;
        }

        public void Update()
        {
            StatusChanged?.Invoke();
        }

        public void OnTechnologyButtonClicked()
        {
            TechnologyButtonClicked?.Invoke();
        }
    }
}
