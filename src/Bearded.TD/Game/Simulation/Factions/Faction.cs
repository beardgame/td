using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Events;
using Bearded.Utilities;
using Bearded.Utilities.Collections;

namespace Bearded.TD.Game.Simulation.Factions
{
    [FactionBehaviorOwner]
    sealed class Faction : IIdable<Faction>
    {
        private readonly IFactionBlueprint blueprint;
        private readonly List<IFactionBehavior<Faction>> behaviors = new();

        public Id<Faction> Id { get; }
        public ExternalId<Faction> ExternalId => blueprint.Id;
        public Faction? Parent { get; }

        public string Name => blueprint.Name ?? "";
        public Color Color => blueprint.Color ?? Color.Black;

        public static Faction FromBlueprint(
            Id<Faction> id, Faction? parent, IFactionBlueprint blueprint, GlobalGameEvents events)
        {
            var faction = new Faction(id, parent, blueprint);
            faction.addBehaviorRange(blueprint.GetBehaviors(), events);
            return faction;
        }

        private Faction(
            Id<Faction> id,
            Faction? parent,
            IFactionBlueprint blueprint)
        {
            Id = id;
            Parent = parent;
            this.blueprint = blueprint;
        }

        private void addBehaviorRange(IEnumerable<IFactionBehavior<Faction>> behaviorsToAdd, GlobalGameEvents events)
        {
            var factionBehaviors = behaviorsToAdd.ToList();
            factionBehaviors.ForEach(behaviors.Add);
            factionBehaviors.ForEach(b => b.OnAdded(this, events));
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
