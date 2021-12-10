using Bearded.TD.Game.Simulation.Reports;

namespace Bearded.TD.Game.Simulation.Damage;

interface IHealthReport : IReport
{
    public HitPoints CurrentHealth { get; }
    public HitPoints MaxHealth { get; }
}