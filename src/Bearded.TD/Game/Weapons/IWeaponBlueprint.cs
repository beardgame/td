using System.Collections.Generic;
using Bearded.TD.Game.Components;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Weapons
{
    interface IWeaponBlueprint : IBlueprint
    {
        TimeSpan NoTargetIdleInterval { get; }
        Unit Range { get; }
        TimeSpan ReCalculateTilesInRangeInterval { get; }

        IEnumerable<IComponent<Weapon>> GetComponents();
    }
}