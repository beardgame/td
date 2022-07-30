using System.Collections.Generic;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Upgrades;

sealed class GameObjectUpgradeSidecar
{
    public GameObject TemplateObject { get; }
    private readonly List<IUpgradeEffect> applicableEffects = new();

    private GameObjectUpgradeSidecar(GameObject templateObject)
    {
        TemplateObject = templateObject;
    }

    public void ApplyUpgrades(GameObject gameObject)
    {
        var upgrade = Upgrade.FromEffects(applicableEffects);
        gameObject.ApplyUpgrade(upgrade);
    }

    public void RegisterEffect(IUpgradeEffect effect)
    {
        applicableEffects.Add(effect);
    }

    public void UnregisterEffect(IUpgradeEffect effect)
    {
        applicableEffects.Remove(effect);
    }

    public static GameObjectUpgradeSidecar ForTemplateObject(GameObject templateObject)
    {
        var sidecar = new GameObjectUpgradeSidecar(templateObject);
        return sidecar;
    }
}
