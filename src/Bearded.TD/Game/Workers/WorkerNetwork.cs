using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;

namespace Bearded.TD.Game.Workers
{
    sealed class WorkerNetwork
    {
        private Tilemap<bool> antennaCoverage;
        private readonly List<IWorkerAntenna> antennae = new List<IWorkerAntenna>();

        public event VoidEventHandler NetworkChanged;

        public void RegisterAntenna(GameState gameState, IWorkerAntenna antenna)
        {
            antennae.Add(antenna);
            buildAntennaCoverage(gameState.Level);
        }

        public void OnAntennaRangeUpdated(GameState gameState)
        {
            buildAntennaCoverage(gameState.Level);
        }

        public void UnregisterAntenna(GameState gameState, IWorkerAntenna antenna)
        {
            var deleted = antennae.Remove(antenna);
            DebugAssert.Argument.Satisfies(deleted);
            buildAntennaCoverage(gameState.Level);
        }

        private void buildAntennaCoverage(Level level)
        {
            DebugAssert.State.Satisfies(antennaCoverage == null || antennaCoverage.Radius == level.Radius);
            antennaCoverage = new Tilemap<bool>(level.Radius);
            foreach (var t in antennaCoverage)
            {
                var pos = Level.GetPosition(t);
                antennaCoverage[t] = antennae.Any(a => (a.Position - pos).LengthSquared < a.WorkerRange.Squared);
            }
            NetworkChanged?.Invoke();
        }

        public bool IsInRange(Tile tile) => antennaCoverage?[tile] ?? false;
    }
}
