using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Content.Models
{
    interface IRuinedParameters : IParametersTemplate<IRuinedParameters>
    {
        ResourceAmount? RepairCost { get; }
    }
}
