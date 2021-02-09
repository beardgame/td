using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bearded.TD.Game.Simulation.Reports
{
    sealed class EmptyReportSubject : IReportSubject
    {
        public IReadOnlyCollection<IReport> Reports { get; } = ImmutableArray<IReport>.Empty;
    }
}
