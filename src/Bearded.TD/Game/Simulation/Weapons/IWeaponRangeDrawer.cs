using System.Collections.Generic;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Weapons;

interface IWeaponRangeDrawer
{
    IEnumerable<Tile> TakeOverDrawingThisFrame();
}