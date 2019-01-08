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
        public GameInstance Game { get; private set; }

        public IPlacedBuilding Building { get; private set; }

        public IEnumerable<UpgradeBlueprint> UpgradesForBuilding
            => Building is Building b
                ? Game.State.Technology.GetApplicableUpgradesFor(b)
                : Enumerable.Empty<UpgradeBlueprint>();

        protected override void Initialize(DependencyResolver dependencies, IPlacedBuilding building)
        {
            Game = dependencies.Resolve<GameInstance>();
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
