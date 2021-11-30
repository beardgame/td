using System;
using Bearded.Utilities.Collections;

namespace Bearded.TD.Game.Simulation.UpdateLoop
{
    interface IPauseCondition : IDeletable {}

    static class PauseCondition
    {
        public static IPauseCondition UntilTrue(Func<bool> predicate) => new PredicatePauseCondition(predicate);

        private sealed class PredicatePauseCondition : IPauseCondition
        {
            private readonly Func<bool> predicate;

            public bool Deleted => predicate();

            public PredicatePauseCondition(Func<bool> predicate)
            {
                this.predicate = predicate;
            }
        }
    }
}
