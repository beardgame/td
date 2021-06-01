using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Tiles;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Workers
{
    [FactionBehavior("workerNetwork")]
    sealed class WorkerNetwork : FactionBehavior<Faction>
    {
        private IArea coverage = Area.Empty();
        private readonly List<IWorkerAntenna> antennae = new();

        protected override void Execute() {}

        public void RegisterAntenna(IWorkerAntenna antenna)
        {
            antennae.Add(antenna);
            buildAntennaCoverage();
        }

        public void OnAntennaRangeUpdated()
        {
            buildAntennaCoverage();
        }

        public void UnregisterAntenna(IWorkerAntenna antenna)
        {
            var deleted = antennae.Remove(antenna);
            Argument.Satisfies(deleted);
            buildAntennaCoverage();
        }

        private void buildAntennaCoverage()
        {
            coverage = Area.Union(antennae.Select(a => a.Coverage));
            Events.Send(new WorkerNetworkChanged(this));
        }

        public bool IsInRange(Tile tile) => coverage.Contains(tile);
    }
}
