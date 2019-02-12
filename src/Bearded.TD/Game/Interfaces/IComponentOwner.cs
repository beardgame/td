using Bearded.TD.Game.Components;

namespace Bearded.TD.Game
{
    interface IComponentOwner<T> where T : IComponentOwner<T>
    {
        TComponent GetComponent<TComponent>() where TComponent : IComponent<T>;
    }
}
