namespace Bearded.TD.Content.Mods
{
    interface IDependencyResolver<out T>
    {
        T Resolve(string id);
    }
}
