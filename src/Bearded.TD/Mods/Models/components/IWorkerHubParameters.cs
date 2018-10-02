using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Mods.Models
{
    interface IWorkerHubParameters : IParametersTemplate
    {
        [Modifiable(Type = ModificationType.DroneCount)] int NumWorkers { get; }
    }
}
