using Bearded.TD.Game.Buildings;
using Bearded.Utilities;

namespace Bearded.TD.Game
{
    // We probably want a better solution for this long-term, but there are so many open questions, I've kept it simple
    // for now.
    //
    // ~ Past Tom
    sealed class GameEvents
    {
        public event GenericEventHandler<BuildingPlaceholder, Building> BuildingConstructionStarted;

        public void StartBuildingConstruction(BuildingPlaceholder placeholder, Building building)
        {
            BuildingConstructionStarted?.Invoke(placeholder, building);
        }
    }
}
