using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;

namespace Bearded.TD.Game.Simulation.Weapons;

interface ITargetingReport : IReport
{
    public GameObject Object { get; }
    public ImmutableArray<ITargetingMode> AvailableTargetingModes { get; }
    public ITargetingMode TargetingMode { get; }
}
