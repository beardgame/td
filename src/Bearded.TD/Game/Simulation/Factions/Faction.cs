using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Game.Simulation.Workers;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Factions
{
    sealed class Faction : IIdable<Faction>
    {
        private readonly Color? color;
        private readonly WorkerNetwork? workerNetwork;
        private readonly WorkerManager? workers;

        private readonly IFactionBlueprint blueprint;
        private readonly List<IFactionBehavior<Faction>> behaviors = new();

        public Id<Faction> Id { get; }
        public Faction? Parent { get; }
        public bool HasResources { get; }
        public bool HasWorkerNetwork { get; }
        public bool HasWorkers { get; }
        public string Name { get; }
        public Color Color => color ?? Parent?.Color ?? Color.Black;
        public FactionResources? Resources =>
            TryGetBehaviorIncludingAncestors<FactionResources>(out var resources) ? resources : null;
        public FactionTechnology? Technology =>
            TryGetBehaviorIncludingAncestors<FactionTechnology>(out var technology) ? technology : null;
        public WorkerNetwork? WorkerNetwork => workerNetwork ?? Parent?.WorkerNetwork;
        public WorkerManager? Workers => workers ?? Parent?.Workers;

        // TODO: do we need an external ID so mod files can refer to other factions?
        public static Faction FromBlueprint(
            Id<Faction> id, string name, IFactionBlueprint blueprint, GlobalGameEvents events)
        {
            var faction = new Faction(id, name, blueprint);
            faction.behaviors.AddRange(blueprint.GetBehaviors());
            faction.behaviors.ForEach(b => b.OnAdded(faction, events));
            return faction;
        }

        private Faction(
            Id<Faction> id,
            string name,
            IFactionBlueprint blueprint)
        {
            Id = id;
            Name = name;
            this.blueprint = blueprint;
        }

        public Faction(
            Id<Faction> id,
            GameState gameState,
            Faction? parent,
            bool hasResources,
            bool hasWorkerNetwork,
            bool hasWorkers,
            string? name = null,
            Color? color = null)
        {
            Id = id;
            Parent = parent;
            HasResources = hasResources;
            HasWorkerNetwork = hasWorkerNetwork;
            HasWorkers = hasWorkers;
            Name = name ?? "";
            this.color = color;

            if (hasResources)
            {
                var resources = new FactionResources();
                behaviors.Add(resources);
                resources.OnAdded(this, gameState.Meta.Events);

                var technology = new FactionTechnology();
                behaviors.Add(technology);
                technology.OnAdded(this, gameState.Meta.Events);
            }
            if (hasWorkerNetwork)
            {
                workerNetwork = new WorkerNetwork();
            }
            if (hasWorkers)
            {
                State.Satisfies(WorkerNetwork != null);
                workers = new WorkerManager(WorkerNetwork!);
            }
        }

        public bool TryGetBehaviorIncludingAncestors<T>([NotNullWhen(true)] out T? result)
        {
            if (TryGetBehavior(out result))
            {
                return true;
            }

            return Parent?.TryGetBehavior(out result) ?? false;
        }

        public bool TryGetBehavior<T>([NotNullWhen(true)] out T? result)
        {
            result = behaviors.OfType<T>().SingleOrDefault();
            return result != null;
        }
    }
}
