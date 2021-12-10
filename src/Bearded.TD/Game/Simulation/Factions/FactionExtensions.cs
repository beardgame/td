namespace Bearded.TD.Game.Simulation.Factions;

static class FactionExtensions
{
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