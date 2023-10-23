using System.Collections.Immutable;
using Bearded.TD.Content.Behaviors;

namespace Bearded.TD.Content.Serialization.Models;

interface IComponent : IBehaviorTemplate
{
    ImmutableArray<string> Keys { get; }
}
