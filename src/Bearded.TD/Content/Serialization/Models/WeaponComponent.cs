namespace Bearded.TD.Content.Serialization.Models
{
    sealed class WeaponComponent<TParameters> : IComponent
    {
        public string Id { get; set; }
        public TParameters Parameters { get; set; }

        object IComponent.Parameters => Parameters;
    }
}
