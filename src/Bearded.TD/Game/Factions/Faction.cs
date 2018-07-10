using amulware.Graphics;
using Bearded.TD.Game.Resources;
using Bearded.Utilities;
using Bearded.Utilities.Collections;

namespace Bearded.TD.Game.Factions
{
    class Faction : IIdable<Faction>
    {
        private readonly Color? color;
        private readonly ResourceManager resources;
        private readonly WorkerManager workers;

        public Id<Faction> Id { get; }
        public Faction Parent { get; }
        public bool HasResources { get; }
        public string Name { get; }
        public Color Color => color ?? Parent?.Color ?? Color.Black;
        public ResourceManager Resources => resources ?? Parent?.Resources;
        public WorkerManager Workers => workers ?? Parent?.Workers;

        public Faction(Id<Faction> id, Faction parent, bool hasResources, string name = null, Color? color = null)
        {
            Id = id;
            Parent = parent;
            HasResources = hasResources;
            Name = name;
            this.color = color;

            if (hasResources)
            {
                resources = new ResourceManager();
                workers = new WorkerManager();
            }
        }
    }
}
