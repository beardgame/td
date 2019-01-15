using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Mods.Models
{
    interface ITargetEnemiesInRange : IParametersTemplate<ITargetEnemiesInRange>
    {
        [Modifiable(Type = AttributeType.Range)] Unit Range { get; }
        [Modifiable(0.2)] TimeSpan NoTargetIdleInterval { get; }
        [Modifiable(1)] TimeSpan ReCalculateTilesInRangeInterval { get; }
    }
}
