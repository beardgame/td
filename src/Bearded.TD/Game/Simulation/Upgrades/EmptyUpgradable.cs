namespace Bearded.TD.Game.Simulation.Upgrades;

sealed class EmptyUpgradable : IUpgradable
{
    public static readonly IUpgradable Instance = new EmptyUpgradable();

    private EmptyUpgradable() {}
    public bool CanApplyUpgrade(IPermanentUpgrade upgrade) => false;
}
