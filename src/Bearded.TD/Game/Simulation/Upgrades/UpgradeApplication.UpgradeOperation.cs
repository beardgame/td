using System;
using System.Collections.Immutable;

namespace Bearded.TD.Game.Simulation.Upgrades;

static partial class UpgradeApplication
{
    private sealed class UpgradeOperation : IUpgradeReceipt
    {
        private readonly IUpgrade upgrade;
        private readonly ImmutableArray<IUpgradeListener> listeners;
        private readonly ImmutableArray<IUpgradeEffectOperation> effectOperations;

        public UpgradeOperation(
            IUpgrade upgrade,
            ImmutableArray<IUpgradeListener> listeners,
            ImmutableArray<IUpgradeEffectOperation> effectOperations)
        {
            this.upgrade = upgrade;
            this.listeners = listeners;
            this.effectOperations = effectOperations;
        }

        public void Commit()
        {
            foreach (var operation in effectOperations)
            {
                operation.Commit();
            }
            foreach (var listener in listeners)
            {
                listener.OnUpgradeCommitted(upgrade);
            }
        }

        public void Rollback()
        {
            foreach (var operation in effectOperations)
            {
                operation.Rollback();
            }
            foreach (var listener in listeners)
            {
                listener.OnUpgradeRolledBack(upgrade);
            }
        }
    }

    private interface IUpgradeEffectOperation
    {
        void Commit();
        void Rollback();
    }

    private sealed class UpgradeEffectOperation<TState> : IUpgradeEffectOperation
    {
        private readonly Func<TState> commit;
        private readonly Action<TState> rollback;
        private bool isCommitted;
        private TState? state;

        public UpgradeEffectOperation(Func<TState> commit, Action<TState> rollback)
        {
            this.commit = commit;
            this.rollback = rollback;
        }

        public void Commit()
        {
            if (isCommitted)
            {
                throw new InvalidOperationException("Can only commit operation once.");
            }

            state = commit();
            isCommitted = true;
        }

        public void Rollback()
        {
            if (!isCommitted)
            {
                throw new InvalidOperationException("Can only roll back a committed operation.");
            }

            rollback(state);
            isCommitted = false;
        }
    }
}
