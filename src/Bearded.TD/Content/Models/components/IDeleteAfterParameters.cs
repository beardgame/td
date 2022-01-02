using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models;

interface IDeleteAfterParameters : IParametersTemplate<IDeleteAfterParameters>
{
    TimeSpan TimeSpan { get; }
}