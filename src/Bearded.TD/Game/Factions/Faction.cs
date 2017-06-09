using amulware.Graphics;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Factions
{
    class Faction : IIdable<Faction>
    {
        private readonly Color? color;

        public Id<Faction> Id { get; }
        public Faction Parent { get; }
        public Color Color => color ?? Parent?.Color ?? Color.Black;

        public Faction(Id<Faction> id, Faction parent, Color? color = null)
        {
            Id = id;
            Parent = parent;
            this.color = color;
        }
    }
}
