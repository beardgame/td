using System.Collections.Generic;
using Bearded.TD.Game.Components;

namespace Bearded.TD.Game.Weapons
{
    interface IWeaponBlueprint : IBlueprint
    {
        IEnumerable<IComponent<Weapon>> GetComponents();
    }
}
