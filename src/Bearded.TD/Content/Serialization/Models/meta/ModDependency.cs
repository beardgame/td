using JetBrains.Annotations;

namespace Bearded.TD.Content.Serialization.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    sealed class ModDependency
    {
        public string? Id { get; set; }
        public string? Alias { get; set; }
    }
}
