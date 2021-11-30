using System.Linq;
using Bearded.TD.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.UpdateLoop
{
    sealed class GameTime : ITimeSource
    {
        public bool IsPaused { get; private set; }
        public Instant Time { get; private set; }

        private readonly DeletableObjectList<IPauseCondition> pauseConditions = new();

        public void Advance(TimeSpan elapsedTime)
        {
            updateIsPaused();
            Time += elapsedTime;
        }

        public void PauseUntil(IPauseCondition condition)
        {
            pauseConditions.Add(condition);
        }

        private void updateIsPaused()
        {
            IsPaused = pauseConditions.Any();
        }
    }
}
