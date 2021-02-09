using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Content.Models
{
    interface IHealthComponentParameter : IParametersTemplate<IHealthComponentParameter>
    {
        [Modifiable(1, Type = AttributeType.Health)]
        HitPoints MaxHealth { get; }

        HitPoints? InitialHealth { get; }
    }
}
