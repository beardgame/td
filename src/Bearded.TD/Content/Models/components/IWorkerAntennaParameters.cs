using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models
{
    public interface IWorkerAntennaParameters : IParametersTemplate<IWorkerAntennaParameters>
    {
        [Modifiable(Type = AttributeType.Range)] Unit WorkerRange { get; }
    }
}
