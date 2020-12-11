using Bearded.TD.Game.Simulation.Factions;

namespace Bearded.TD.Game.Simulation.Damage
{
    interface IDamageOwner
    {
        Faction Faction { get; }
    }
}
