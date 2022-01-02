using Bearded.TD.Content.Behaviors;
using JetBrains.Annotations;

namespace Bearded.TD.Content.Serialization.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
sealed class NodeBehavior<TParameters> : INodeBehavior
{
    public string? Id { get; set; }
    public TParameters? Parameters { get; set; }

    object? IBehaviorTemplate.Parameters => Parameters;
}