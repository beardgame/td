using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models;

interface ITileVisibilityParameters : IParametersTemplate<ITileVisibilityParameters>
{
    Unit Range { get; }
}