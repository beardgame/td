using System.Collections.Generic;
using JetBrains.Annotations;

namespace Bearded.TD.Content.Serialization.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    sealed class Metadata
    {
        public string? Name { get; set; }
        public string? Id { get; set; }
        public List<ModDependency>? Dependencies { get; set; }
    }
}
