using System.Collections.Immutable;

namespace Bearded.TD.Game.Simulation.Weapons;

interface ITargetingModeSetter
{
    void SetTargetingMode(ITargetingMode newMode);
    ImmutableArray<ITargetingMode> AllowedTargetingModes { get; }
}
