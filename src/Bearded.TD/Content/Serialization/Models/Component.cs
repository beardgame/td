
using Bearded.TD.Content.Behaviors;

namespace Bearded.TD.Content.Serialization.Models
{
    class Component<TParameters> : IComponent
    {
        public string Id { get; set; }
        public TParameters Parameters { get; set; }

        object IBehaviorTemplate.Parameters => Parameters;
    }
}
