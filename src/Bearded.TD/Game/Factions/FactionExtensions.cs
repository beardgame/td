namespace Bearded.TD.Game.Factions
{
    static class FactionExtensions
    {
        public static bool IsAncestorOf(this Faction toCheck, Faction potentialAncestor)
        {
            for (var current = toCheck; current != null; current = current.Parent)
            {
                if (current == potentialAncestor) return true;
            }
            return false;
        }

        public static bool IsDescendantOf(this Faction toCheck, Faction potentialDescendant) =>
            IsAncestorOf(potentialDescendant, toCheck);

        public static bool SharesWorkersWith(this Faction thisFaction, Faction thatFaction) =>
            thisFaction.Workers != null && thisFaction.Workers == thatFaction.Workers;
    }
}
