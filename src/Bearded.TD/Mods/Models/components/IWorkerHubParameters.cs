using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Mods.Models
{
    interface IWorkerHubParameters : ITechEffectModifiable
    {
        [Modifiable] int NumWorkers { get; }
    }
}
