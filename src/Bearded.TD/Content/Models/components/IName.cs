using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Content.Models;

interface IName : IParametersTemplate<IName>
{
    public string Name { get; }
}