using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Mods.Models
{
    interface ITileVisibilityParameters : IParametersTemplate
    {
        [Modifiable] Unit Range { get; }
    }
}
