using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Mods.Models
{
    interface IIncomeOverTimeParameters : ITechEffectModifiable
    {
        [Modifiable]
        float IncomePerSecond { get; }
    }
}
