using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Technologies;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.UI.Controls
{
    sealed class TechnologyUIModel
    {
        public enum TechnologyStatus
        {
            Unlocked,
            Queued,
            CanBeUnlocked,
            MissingResources,
            MissingDependencies
        }

        public ImmutableList<ITechnologyBlueprint> Technologies { get; }
        private readonly TechnologyManager technologyManager;

        private readonly IDictionary<ITechnologyBlueprint, TechnologyStatus> technologyStatuses;
        private readonly ImmutableDictionary<ITechnologyBlueprint, IList<ITechnologyBlueprint>> technologyDependents;

        public TechnologyUIModel(
            IEnumerable<ITechnologyBlueprint> technologies, TechnologyManager technologyManager)
        {
            Technologies = technologies.ToImmutableList();
            this.technologyManager = technologyManager;
            technologyStatuses = Technologies.ToDictionary(t => t, evaluateStatusForTech);
            technologyDependents = buildTechnologyDependents();
        }

        public void Update()
        {
            foreach (var tech in Technologies.Where(tech => StatusFor(tech) != TechnologyStatus.Unlocked))
            {
                UpdateTechnology(tech);
            }
        }

        public void UpdateTechnology(ITechnologyBlueprint tech)
        {
            technologyStatuses[tech] = evaluateStatusForTech(tech);
        }

        public TechnologyStatus StatusFor(ITechnologyBlueprint tech) => technologyStatuses[tech];

        public IEnumerable<ITechnologyBlueprint> DependentsFor(ITechnologyBlueprint tech) => technologyDependents[tech];

        private TechnologyStatus evaluateStatusForTech(ITechnologyBlueprint tech)
        {
            if (!technologyManager.IsTechnologyLocked(tech))
            {
                return TechnologyStatus.Unlocked;
            }

            if (technologyManager.CanUnlockTechnology(tech))
            {
                return TechnologyStatus.CanBeUnlocked;
            }

            if (technologyManager.IsTechnologyQueued(tech))
            {
                return TechnologyStatus.Queued;
            }

            return technologyManager.HasAllRequiredTechs(tech)
                ? TechnologyStatus.MissingResources
                : TechnologyStatus.MissingDependencies;
        }

        private ImmutableDictionary<ITechnologyBlueprint, IList<ITechnologyBlueprint>> buildTechnologyDependents()
        {
            var accumulatedDict = new MultiDictionary<ITechnologyBlueprint, ITechnologyBlueprint>();

            foreach (var technology in Technologies)
            {
                foreach (var requiredTech in technology.RequiredTechs)
                {
                    accumulatedDict.Add(requiredTech, technology);
                }
            }

            return accumulatedDict.ToImmutableDictionary();
        }
    }
}
