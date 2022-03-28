using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models;

interface ITurretParameters : IParametersTemplate<ITurretParameters>
{
    IComponentOwnerBlueprint Weapon { get; }
    Difference2 Offset { get; }

    [Modifiable(0.25)]
    Unit Height { get; }

    Direction2 NeutralDirection { get; }
    Angle? MaximumTurningAngle { get; }
}
