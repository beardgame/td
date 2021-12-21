using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Content.Models;

interface IThreat : IParametersTemplate<IThreat>
{
    [Modifiable]
    float Threat { get; }
}
