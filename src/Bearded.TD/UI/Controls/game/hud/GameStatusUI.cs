using Bearded.TD.Game;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    sealed class GameStatusUI
    {
        public VoidEventHandler StatusChanged;

        private GameInstance game;

        public long FactionResources => game.Me.Faction.Resources.CurrentResources;
        public int FactionResourceIncome => game.Me.Faction.Resources.CurrentIncome;
        public long FactionTechPoints => game.State.Technology.TechPoints;

        public void Initialize(GameInstance game)
        {
            this.game = game;
        }

        public void Update()
        {
            StatusChanged?.Invoke();
        }
    }
}
