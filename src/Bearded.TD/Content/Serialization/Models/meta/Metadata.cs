using System.Collections.Generic;

namespace Bearded.TD.Content.Serialization.Models
{
    sealed class Metadata
    {
        public string? Name { get; set; }
        public string? Id { get; set; }
        public List<ModDependency>? Dependencies { get; set; }
    }
}
