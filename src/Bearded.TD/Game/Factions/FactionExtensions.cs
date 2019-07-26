namespace Bearded.TD.Game.Factions
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

        public static bool IsDescendantOf(this Faction toCheck, Faction potentialDescendant) =>
            IsAncestorOf(potentialDescendant, toCheck);

        public static bool SharesResourcesWith(this Faction thisFaction, Faction thatFaction) =>
            thisFaction.Resources != null && thisFaction.Resources == thatFaction.Resources;

        public static bool SharesWorkersWith(this Faction thisFaction, Faction thatFaction) =>
            thisFaction.Workers != null && thisFaction.Workers == thatFaction.Workers;
    }
}
