using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;

namespace Bearded.TD.Game.Simulation.Upgrades;

interface IUpgradePreview
{
    void RegisterGameObject(GameObject gameObject);
    void RegisterParameters(GameObject gameObject, IParametersTemplate parameters);
    void RegisterListener(GameObject gameObject, IUpgradeListener listener);
}
