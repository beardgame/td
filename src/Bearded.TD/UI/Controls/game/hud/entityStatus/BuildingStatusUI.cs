using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.UI.Controls;
using Bearded.UI.Navigation;

namespace Bearded.TD.UI.Controls
{
    sealed class BuildingStatusUI : NavigationNode<IPlacedBuilding>
    {
        public GameInstance Game { get; private set; }

        public IPlacedBuilding Building { get; private set; }

        public IEnumerable<IUpgradeBlueprint> UpgradesForBuilding
            => Building is Building b
                ? b.GetApplicableUpgrades()
                : Enumerable.Empty<IUpgradeBlueprint>();

        protected override void Initialize(DependencyResolver dependencies, IPlacedBuilding building)
        {
            Game = dependencies.Resolve<GameInstance>();
            Building = building;

            Building.Deleting += Navigation.Exit;
        }

        public override void Terminate()
        {
            base.Terminate();

            Building.Deleting -= Navigation.Exit;
        }

        public void OnCloseClicked(Button.ClickEventArgs _)
        {
            Navigation.Exit();
        }

        public void OnDeleteBuildingClicked(Button.ClickEventArgs _)
        {
            switch (Building)
            {
                case BuildingPlaceholder placeholder:
                    Game.Request(placeholder.CancelRequest());
                    break;
                case Building _:
                    // TODO: implement
                    break;
            }
        }
    }
}
