using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Simulation.Upgrades;

interface IUpgradePreview
{
    IUpgrade Upgrade { get; }

    void RegisterGameObject(GameObject gameObject);
    void RegisterParameters(GameObject gameObject, IParametersTemplate parameters);
}
