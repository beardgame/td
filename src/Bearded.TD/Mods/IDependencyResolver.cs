namespace Bearded.TD.Mods
{
    interface IDependencyResolver<out T>
    {
        T Resolve(string id);
    }
}
