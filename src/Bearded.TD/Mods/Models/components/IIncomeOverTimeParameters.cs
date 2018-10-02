using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Mods.Models
{
    interface IIncomeOverTimeParameters : IParametersTemplate
    {
        [Modifiable] float IncomePerSecond { get; }
    }
}
