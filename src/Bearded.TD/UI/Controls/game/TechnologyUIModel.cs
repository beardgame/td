using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Technologies;
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

        private readonly GameInstance game;
        private readonly Faction faction;
        private readonly TechnologyManager technologyManager;

        private readonly IDictionary<ITechnologyBlueprint, TechnologyStatus> technologyStatuses;
        private readonly ImmutableDictionary<ITechnologyBlueprint, IList<ITechnologyBlueprint>> technologyDependents;

        public ImmutableArray<ITechnologyBlueprint> Technologies { get; }

        public TechnologyUIModel(GameInstance game)
        {
            this.game = game;
            Technologies = game.Blueprints.Technologies.All.ToImmutableArray();
            faction = game.Me.Faction;
            technologyManager = faction.Technology;

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

        public long CostFor(ITechnologyBlueprint tech) => technologyManager.ExpectedCost(tech);

        public void UpdateTechnology(ITechnologyBlueprint tech)
        {
            technologyStatuses[tech] = evaluateStatusForTech(tech);
        }

        public void ReplaceTechnologyQueue(ITechnologyBlueprint tech)
        {
            game.Request(Game.Commands.Gameplay.ReplaceTechnologyQueue.Request(faction, tech));
        }

        public void AddToTechnologyQueue(ITechnologyBlueprint tech)
        {
            game.Request(Game.Commands.Gameplay.AddToTechnologyQueue.Request(faction, tech));
        }

        public void ClearTechnologyQueue()
        {
            game.Request(Game.Commands.Gameplay.ClearTechnologyQueue.Request(faction));
        }

        public TechnologyStatus StatusFor(ITechnologyBlueprint tech) => technologyStatuses[tech];

        public int QueuePositionFor(ITechnologyBlueprint tech) => technologyManager.QueuePositionFor(tech);

        public IEnumerable<ITechnologyBlueprint> DependentsFor(ITechnologyBlueprint tech) =>
            technologyDependents.GetValueOrDefault(tech, ImmutableArray<ITechnologyBlueprint>.Empty);

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
