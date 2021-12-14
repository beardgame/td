using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models;

interface IMoveToBaseParameters : IParametersTemplate<IMoveToBaseParameters>
{
    [Modifiable(Type = AttributeType.MovementSpeed)]
    Speed MovementSpeed { get; }
}