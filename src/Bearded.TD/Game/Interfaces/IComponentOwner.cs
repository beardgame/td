using System.Collections.Generic;
using Bearded.TD.Game.Components;

namespace Bearded.TD.Game
{
    interface IComponentOwner
    {
        IEnumerable<T> GetComponents<T>();
    }
    
    interface IComponentOwner<T> : IComponentOwner where T : IComponentOwner<T>
    {
        new IEnumerable<TComponent> GetComponents<TComponent>() where TComponent : IComponent<T>;
    }
}
