using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Simulation.Upgrades;

interface IUpgradePreview
{
    void RegisterGameObject(GameObject gameObject);
    void RegisterParameters(GameObject gameObject, IParametersTemplate parameters);
    void RegisterListener(GameObject gameObject, IUpgradeListener listener);
}
