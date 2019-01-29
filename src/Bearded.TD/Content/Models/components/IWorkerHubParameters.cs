using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Content.Models
{
    interface IWorkerHubParameters : IParametersTemplate<IWorkerHubParameters>
    {
        [Modifiable(Type = AttributeType.DroneCount)] int NumWorkers { get; }
    }
}
