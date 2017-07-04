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

        public Id<Faction> Id { get; }
        public Faction Parent { get; }
        public bool HasResources { get; }
        public Color Color => color ?? Parent?.Color ?? Color.Black;
        public ResourceManager Resources => resources ?? Parent?.Resources;

        public Faction(Id<Faction> id, Faction parent, bool hasResources, Color? color = null)
        {
            Id = id;
            Parent = parent;
            HasResources = hasResources;
            this.color = color;

            if (hasResources)
                resources = new ResourceManager();
        }
    }
}
