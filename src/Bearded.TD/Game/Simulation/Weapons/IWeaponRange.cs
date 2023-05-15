using System.Collections.Immutable;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

interface IWeaponRange
{
    Unit Range { get; }

    ImmutableArray<Tile> GetTilesInRange();
}
