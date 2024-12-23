using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Upgrades;

interface ICommittedUpgrade
{
    IUpgrade Upgrade { get; }
    public void Amend(IComponent component);
}
