using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models
{
    interface IWaterGeneratorParameters : IParametersTemplate<IWaterGeneratorParameters>
    {
        [Modifiable]
        FlowRate VolumePerSecond { get; }
    }
}
