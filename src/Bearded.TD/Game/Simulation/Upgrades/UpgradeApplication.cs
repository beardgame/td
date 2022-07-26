using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities.Collections;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.Game.Simulation.Upgrades;

static class UpgradeApplication
{
    public static bool CanApplyUpgrade(this GameObject subject, IUpgrade upgrade)
    {
        var upgradePreview = new UpgradePreview(upgrade);
        subject.PreviewUpgrade(upgradePreview);
        return upgradePreview.WouldBeEffective();
    }

    public static IUpgradeReceipt ApplyUpgrade(this GameObject subject, IUpgrade upgrade)
    {
        var upgradePreview = new UpgradePreview(upgrade);
        subject.PreviewUpgrade(upgradePreview);
        var operation = upgradePreview.ToOperation();
        operation.Commit();
        return operation;
    }

    private sealed class UpgradePreview : IUpgradePreview
    {
        private readonly MultiDictionary<UpgradeEffectCandidate, IUpgradeEffectOperation> upgradeCandidates = new();

        public IUpgrade Upgrade { get; }

        public UpgradePreview(IUpgrade upgrade)
        {
            Upgrade = upgrade;
        }

        public void RegisterGameObject(GameObject gameObject)
        {
            foreach (var effect in Upgrade.Effects.Where(e => e.ContributesComponent))
            {
                upgradeCandidates.Add(
                    new UpgradeEffectCandidate(gameObject, effect),
                    new UpgradeEffectOperation<IComponent>(
                        () =>
                        {
                            var component = effect.CreateComponent();
                            gameObject.AddComponent(component);
                            return component;
                        },
                        gameObject.RemoveComponent));
            }
        }

        public void RegisterParameters(GameObject gameObject, IParametersTemplate parameters)
        {
            foreach (var effect in Upgrade.Effects.Where(e => e.CanApplyTo(parameters)))
            {
                upgradeCandidates.Add(
                    new UpgradeEffectCandidate(gameObject, effect),
                    new UpgradeEffectOperation<Void>(
                        () =>
                        {
                            effect.ApplyTo(parameters);
                            return default;
                        },
                        _ => effect.RemoveFrom(parameters)));
            }
        }

        public bool WouldBeEffective()
        {
            foreach (var (candidate, _) in upgradeCandidates)
            {
                if (!candidate.UpgradeEffect.IsSideEffect && candidate.SatisfiesPrerequisites())
                {
                    return true;
                }
            }

            return false;
        }

        public UpgradeOperation ToOperation()
        {
            var effectOperations = new List<IUpgradeEffectOperation>();

            foreach (var (candidate, ops) in upgradeCandidates)
            {
                if (candidate.SatisfiesPrerequisites())
                {
                    effectOperations.AddRange(ops);
                }
            }

            return new UpgradeOperation(effectOperations.ToImmutableArray());
        }
    }

    private readonly record struct UpgradeEffectCandidate(GameObject GameObject, IUpgradeEffect UpgradeEffect)
    {
        public bool SatisfiesPrerequisites()
        {
            var prerequisites = UpgradeEffect.Prerequisites;
            var tags = GameObject.Tags();
            return prerequisites.RequiredTags.IsSubsetOf(tags) && !prerequisites.ForbiddenTags.Overlaps(tags);
        }
    }

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
}
