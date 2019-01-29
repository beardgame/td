
namespace Bearded.TD.Content.Serialization.Models
{
    class Component<TParameters> : IComponent
    {
        public string Id { get; set; }
        public TParameters Parameters { get; set; }

        object IComponent.Parameters => Parameters;
    }
}
