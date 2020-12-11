using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Content.Models
{
    interface IIncomeOverTimeParameters : IParametersTemplate<IIncomeOverTimeParameters>
    {
        [Modifiable(Type = AttributeType.ResourceIncome)]
        ResourceRate IncomePerSecond { get; }
    }
}
