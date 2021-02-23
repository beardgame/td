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
    }
}
