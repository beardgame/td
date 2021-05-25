using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Bearded.Graphics;
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
        private readonly ResourceManager? resources;
        private readonly TechnologyManager? technology;
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
        public ResourceManager? Resources => resources ?? Parent?.Resources;
        public TechnologyManager? Technology => technology ?? Parent?.Technology;
        public WorkerNetwork? WorkerNetwork => workerNetwork ?? Parent?.WorkerNetwork;
        public WorkerManager? Workers => workers ?? Parent?.Workers;

        // TODO: do we need an external ID so mod files can refer to other factions?
        public static Faction FromBlueprint(Id<Faction> id, string name, IFactionBlueprint blueprint)
        {
            var faction = new Faction(id, name, blueprint);
            faction.behaviors.AddRange(blueprint.GetBehaviors());
            // TODO: initialize behaviors
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
                resources = new ResourceManager(gameState.Meta.Logger);
                technology = new TechnologyManager(gameState.Meta.Events);
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

        // TODO: instead of always just looking up the ancestors for a faction, should we instead make inheritance
        //       explicit? For example: instead of having a single "WorkerNetwork" behavior, we could have an interface
        //       for getting the worker network on a faction, and have an actual implementation, as well as one that
        //       proxies to the parent worker network (if it exists).
        //       Pros: supports caching the found parent network, makes things more explicit, allows factions to have
        //             multiple parents, can have a faction between the ancestor and child that doesn't have access to
        //             the relevant inherited thing
        //       Cons: maybe makes no sense to have a faction between two other factions without access to the same
        //             things, mod files always need to specify when something is inherited
        //       Which approach makes it easier to vary the existence of these behaviors based on the game rules?
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
