using Bearded.TD.Game.Weapons;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models
{
    interface ITurretParameters : IParametersTemplate<ITurretParameters>
    {
        IWeaponBlueprint Weapon { get; }
        Difference2 Offset { get; }
        Direction2 NeutralDirection { get; }
        Angle? MaximumTurningAngle { get; }
    }
}
