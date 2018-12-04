using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Upgrades;
using Bearded.UI.Navigation;

namespace Bearded.TD.UI.Controls
{
    sealed class BuildingStatusUI : NavigationNode<IPlacedBuilding>
    {
        private GameInstance game;

        public IPlacedBuilding Building { get; private set; }

        public IEnumerable<UpgradeBlueprint> UpgradesForBuilding
            => Building is Building b
                ? game.State.Technology.GetApplicableUpgradesFor(b)
                : Enumerable.Empty<UpgradeBlueprint>();

        protected override void Initialize(DependencyResolver dependencies, IPlacedBuilding building)
        {
            this.game = dependencies.Resolve<GameInstance>();
            Building = building;
        }

        public void OnCloseClicked()
        {
            Navigation.Exit();
        }

        public void OnDeleteBuildingClicked()
        {

        }
    }
}
