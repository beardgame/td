using Bearded.TD.Game.Components;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models
{
    interface ITurretParameters : IParametersTemplate<ITurretParameters>
    {
        IComponentOwnerBlueprint Weapon { get; }
        Difference2 Offset { get; }
        Direction2 NeutralDirection { get; }
        Angle? MaximumTurningAngle { get; }
    }
}
