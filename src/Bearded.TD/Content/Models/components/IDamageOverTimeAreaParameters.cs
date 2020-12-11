using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models
{
    interface IDamageOverTimeAreaParameters : IParametersTemplate<IDamageOverTimeAreaParameters>
    {
        [Modifiable(10, Type = AttributeType.Damage)]
        int DamagePerSecond { get; }

        DamageType Type { get; }

        [Modifiable(2, Type = AttributeType.Range)]
        Unit Range { get; }
    }
}
