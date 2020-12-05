using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Resources;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    sealed class GameStatusUI
    {
        public event VoidEventHandler? StatusChanged;

        private GameInstance game;

        public string FactionName => game.Me.Faction.Name;
        public Color FactionColor => game.Me.Faction.Color;
        public ResourceAmount FactionResources => game.Me.Faction.Resources.CurrentResources;
        public ResourceRate FactionResourceIncome => game.Me.Faction.Resources.CurrentIncome;
        public long FactionTechPoints => game.Me.Faction.Technology.TechPoints;

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
