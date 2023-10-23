using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameObjects;

interface IComponent
{
    ImmutableArray<string> Keys { get; init; }

    void OnAdded(GameObject owner, ComponentEvents events);
    void Activate();
    void Update(TimeSpan elapsedTime);
    void OnRemoved();

    void PreviewUpgrade(IUpgradePreview upgradePreview);
}
