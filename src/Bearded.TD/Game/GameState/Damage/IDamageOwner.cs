using Bearded.TD.Game.GameState.Factions;

namespace Bearded.TD.Game.GameState.Damage
{
    interface IDamageOwner
    {
        Faction Faction { get; }
    }
}
