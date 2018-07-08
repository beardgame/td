using Bearded.TD.Game;
using Bearded.TD.Game.Buildings;

namespace Bearded.TD.UI.Controls
{
    sealed class BuildingStatusUI
    {
        private GameInstance game;

        public IPlacedBuilding Building { get; private set; }

        public void Initialize(GameInstance game, IPlacedBuilding building)
        {
            this.game = game;
            Building = building;
        }

        public void OnCloseClicked()
        {

        }

        public void OnDeleteBuildingClicked()
        {

        }
    }
}
