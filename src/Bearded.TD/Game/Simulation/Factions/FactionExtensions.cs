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
            thisFaction.Resources != null && thisFaction.Resources == thatFaction.Resources;

        public static bool SharesTechnologyWith(this Faction thisFaction, Faction thatFaction) =>
            thisFaction.Technology != null && thisFaction.Technology == thatFaction.Technology;

        public static bool SharesWorkersWith(this Faction thisFaction, Faction thatFaction) =>
            thisFaction.Workers != null && thisFaction.Workers == thatFaction.Workers;

        public static bool SharesWorkerNetworkWith(this Faction thisFaction, Faction thatFaction) =>
            thisFaction.WorkerNetwork != null && thisFaction.WorkerNetwork == thatFaction.WorkerNetwork;
    }
}
