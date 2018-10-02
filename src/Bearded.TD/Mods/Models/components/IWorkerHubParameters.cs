using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Mods.Models
{
    interface IWorkerHubParameters : ITechEffectModifiable
    {
        [Modifiable(Type = ModificationType.DroneCount)] int NumWorkers { get; }
    }
}
