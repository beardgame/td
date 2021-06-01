using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Game.Simulation.Workers;

namespace Bearded.TD.Game.Simulation.Factions
{
    static class FactionExtensions
    {
        public static bool IsAncestorOf(this Faction thisFaction, Faction thatFaction)
        {
            for (var current = thatFaction; current != null; current = current.Parent)
            {
                if (current == thisFaction) return true;
            }
            return false;
        }

        public static bool IsDescendantOf(this Faction toCheck, Faction potentialAncestor) =>
            IsAncestorOf(potentialAncestor, toCheck);

        public static bool SharesResourcesWith(this Faction thisFaction, Faction thatFaction) =>
            thisFaction.SharesBehaviorWith<FactionResources>(thatFaction);

        public static bool SharesTechnologyWith(this Faction thisFaction, Faction thatFaction) =>
            thisFaction.SharesBehaviorWith<FactionTechnology>(thatFaction);

        public static bool SharesWorkersWith(this Faction thisFaction, Faction thatFaction) =>
            thisFaction.SharesBehaviorWith<WorkerTaskManager>(thatFaction);

        public static bool SharesWorkerNetworkWith(this Faction thisFaction, Faction thatFaction) =>
            thisFaction.SharesBehaviorWith<WorkerNetwork>(thatFaction);

        public static bool SharesBehaviorWith<TBehavior>(this Faction thisFaction, Faction thatFaction)
        {
            if (!thisFaction.TryGetBehaviorIncludingAncestors<TBehavior>(out var thisBehavior))
            {
                return false;
            }
            thatFaction.TryGetBehaviorIncludingAncestors<TBehavior>(out var thatBehavior);
            return Equals(thisBehavior, thatBehavior);
        }
    }
}
