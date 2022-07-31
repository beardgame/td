using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities.Collections;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.Game.Simulation.Upgrades;

static partial class UpgradeApplication
{
    private sealed class UpgradePreview : IUpgradePreview
    {
        private readonly IUpgrade upgrade;
        private readonly MultiDictionary<GameObject, IUpgradeListener> listeners = new();
        private readonly MultiDictionary<UpgradeEffectCandidate, IUpgradeEffectOperation> upgradeCandidates = new();

        public UpgradePreview(IUpgrade upgrade)
        {
            this.upgrade = upgrade;
        }

        public void RegisterGameObject(GameObject gameObject)
        {
            foreach (var effect in upgrade.Effects.Where(e => e.ContributesComponent))
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
            foreach (var effect in upgrade.Effects.Where(e => e.CanApplyTo(parameters)))
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

        public void RegisterListener(GameObject gameObject, IUpgradeListener listener)
        {
            listeners.Add(gameObject, listener);
        }

        public bool WouldBeEffective()
        {
            return upgradeCandidates
                .Select(kvp => kvp.Key)
                .Any(candidate => !candidate.UpgradeEffect.IsSideEffect && candidate.SatisfiesPrerequisites());
        }

        public UpgradeOperation ToOperation()
        {
            var effectOperations = new List<IUpgradeEffectOperation>();
            var affectedGameObjects = new HashSet<GameObject>();

            foreach (var (candidate, ops) in upgradeCandidates)
            {
                if (candidate.SatisfiesPrerequisites())
                {
                    effectOperations.AddRange(ops);
                    affectedGameObjects.Add(candidate.GameObject);
                }
            }

            var affectedListeners = affectedGameObjects
                .SelectMany(gameObjectWithParents)
                .Distinct()
                .SelectMany(obj => listeners[obj])
                .ToImmutableArray();

            return new UpgradeOperation(upgrade, affectedListeners, effectOperations.ToImmutableArray());
        }

        private IEnumerable<GameObject> gameObjectWithParents(GameObject start)
        {
            var current = start;
            while (current != null)
            {
                yield return current;
                current = current.Parent;
            }
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
}
