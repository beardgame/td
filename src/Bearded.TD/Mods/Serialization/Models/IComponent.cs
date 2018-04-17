namespace Bearded.TD.Mods.Serialization.Models
{
    interface IComponent
    {
        string Id { get; }
        object Parameters { get; }
    }
}
