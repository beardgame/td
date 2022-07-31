using System.Collections.Generic;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Upgrades;

sealed class GameObjectUpgradeCopier : IUpgradeListener
{
    private readonly GameObject templateObject;
    private readonly List<IUpgrade> appliedUpgrades = new();

    private GameObjectUpgradeCopier(GameObject templateObject)
    {
        this.templateObject = templateObject;
    }

    public void PreviewUpgrade(IUpgradePreview upgradePreview)
    {
        templateObject.PreviewUpgrade(upgradePreview);
        upgradePreview.RegisterListener(templateObject, this);
    }

    public void OnUpgradeCommitted(IUpgrade upgrade)
    {
        appliedUpgrades.Add(upgrade);
    }

    public void OnUpgradeRolledBack(IUpgrade upgrade)
    {
        appliedUpgrades.Remove(upgrade);
    }

    public void CopyUpgradesTo(GameObject gameObject)
    {
        foreach (var upgrade in appliedUpgrades)
        {
            gameObject.ApplyUpgrade(upgrade);
        }
    }

    public static GameObjectUpgradeCopier ForTemplateObject(GameObject templateObject)
    {
        var copier = new GameObjectUpgradeCopier(templateObject);
        return copier;
    }
}
