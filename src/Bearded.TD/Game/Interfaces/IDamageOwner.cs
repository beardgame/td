using Bearded.TD.Game.Factions;

namespace Bearded.TD.Game
{
    interface IDamageOwner
    {
        Faction Faction { get; }
    }
}
