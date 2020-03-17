namespace Bearded.TD.Content.Serialization.Models
{
    interface IComponent
    {
        string? Id { get; }
        object? Parameters { get; }
    }
}
