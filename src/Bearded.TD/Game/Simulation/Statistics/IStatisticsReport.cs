using Bearded.TD.Game.Simulation.Reports;

namespace Bearded.TD.Game.Simulation.Statistics;

interface IStatisticsReport : IReport
{
    public long TotalDamage { get; }
    public long TotalKills { get; }

    public long CurrentWaveDamage { get; }
    public long CurrentWaveKills { get; }

    public long PreviousWaveDamage { get; }
    public long PreviousWaveKills { get; }
}
