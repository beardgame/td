using System;
using System.Collections.Immutable;
using Bearded.TD.Utilities.Collections;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.Game.Simulation.Upgrades;

static partial class UpgradeApplication
{
    private sealed class UpgradeOperation : IUpgradeReceipt
    {
        private readonly ImmutableArray<IUpgradeEffectOperation> effectOperations;

        public UpgradeOperation(ImmutableArray<IUpgradeEffectOperation> effectOperations)
        {
            this.effectOperations = effectOperations;
        }

        public void Commit() => effectOperations.ForEach(op => op.Commit());
        public void Rollback() => effectOperations.ForEach(op => op.Rollback());
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
            if (!isCommitted)
            {
                throw new InvalidOperationException("Can only commit operation once.");
            }

            state = commit();
            isCommitted = true;
        }

        public void Rollback()
        {
            if (isCommitted)
            {
                throw new InvalidOperationException("Can only roll back a committed operation.");
            }

            rollback(state);
            isCommitted = false;
        }
    }

    private static IUpgradeEffectOperation compose(IUpgradeEffectOperation left, IUpgradeEffectOperation right)
    {
        return new UpgradeEffectOperation<Void>(
            () =>
            {
                left.Commit();
                right.Commit();
                return default;
            },
            _ =>
            {
                left.Rollback();
                right.Rollback();
            });
    }
}
