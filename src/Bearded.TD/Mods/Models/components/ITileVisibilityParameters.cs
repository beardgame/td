using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Mods.Models
{
    interface ITileVisibilityParameters : ITechEffectModifiable
    {
        [Modifiable]
        Unit Range { get; }
    }
}
