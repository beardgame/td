using System.Collections.Generic;

namespace Bearded.TD.Game.Simulation.Reports
{
    interface IReportSubject
    {
        public IReadOnlyCollection<IReport> Reports { get; }
    }
}
