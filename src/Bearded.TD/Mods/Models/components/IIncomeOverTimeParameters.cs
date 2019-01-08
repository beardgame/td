using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Mods.Models
{
    interface IIncomeOverTimeParameters : IParametersTemplate<IIncomeOverTimeParameters>
    {
        [Modifiable(Type = AttributeType.ResourceIncome)]
        float IncomePerSecond { get; }
    }
}
