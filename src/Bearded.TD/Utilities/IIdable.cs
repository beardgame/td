namespace Bearded.TD.Utilities
{
    interface IIdable<T>
    {
        Id<T> Id { get; }
    }
}