using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Upgrades;

static partial class UpgradeApplication
{
    private sealed class UpgradeOperation : IUpgradeReceipt
    {
        private readonly IUpgrade upgrade;
        private readonly List<GameObject> gameObjects = [];
        private readonly List<IUpgradeListener> listeners = [];
        private readonly List<IUpgradeEffectOperation> effectOperations = [];

        public UpgradeOperation(
            IUpgrade upgrade,
            ImmutableArray<GameObject> gameObjects,
            ImmutableArray<IUpgradeListener> listeners,
            ImmutableArray<IUpgradeEffectOperation> effectOperations)
        {
            this.upgrade = upgrade;
            this.gameObjects.AddRange(gameObjects);
            this.listeners.AddRange(listeners);
            this.effectOperations.AddRange(effectOperations);
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
            foreach (var gameObject in gameObjects)
            {
                gameObject.OnUpgradeCommitted(new CommittedUpgrade(this));
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
            foreach (var gameObject in gameObjects)
            {
                gameObject.OnUpgradeRolledBack(upgrade);
            }
        }

        private sealed class CommittedUpgrade(UpgradeOperation operation) : ICommittedUpgrade
        {
            public IUpgrade Upgrade => operation.upgrade;

            public void Amend(IComponent component)
            {
                var upgradePreview = new UpgradePreview(Upgrade);
                component.PreviewUpgrade(upgradePreview);
                if (!upgradePreview.WouldBeEffective()) return;

                var amendment = upgradePreview.ToOperation();
                var existingEffectKeys = operation.effectOperations.Select(o => o.Key).ToImmutableHashSet();

                foreach (var op in amendment.effectOperations.Where(op => !existingEffectKeys.Contains(op.Key)))
                {
                    op.Commit();
                    operation.effectOperations.Add(op);
                }
                foreach (var listener in amendment.listeners.Where(listener => !operation.listeners.Contains(listener)))
                {
                    listener.OnUpgradeCommitted(Upgrade);
                    operation.listeners.Add(listener);
                }
                foreach (var gameObject in amendment.gameObjects.Where(obj => !operation.gameObjects.Contains(obj)))
                {
                    gameObject.OnUpgradeCommitted(this);
                    operation.gameObjects.Add(gameObject);
                }
            }
        }
    }

    private interface IUpgradeEffectOperation
    {
        object Key { get; }

        void Commit();
        void Rollback();
    }

    private sealed class UpgradeEffectOperation<TState> : IUpgradeEffectOperation
    {
        public object Key { get; }

        private readonly Func<TState> commit;
        private readonly Action<TState> rollback;
        private bool isCommitted;
        private TState? state;

        public UpgradeEffectOperation(object key, Func<TState> commit, Action<TState> rollback)
        {
            Key = key;
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

            rollback(state!);
            isCommitted = false;
        }
    }
}
