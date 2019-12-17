using System.Collections.Generic;
using Bearded.TD.Game.Components;
using Bearded.Utilities;

namespace Bearded.TD.Game
{
    interface IComponentOwner
    {
        Maybe<IComponentOwner> Parent { get; }

        IEnumerable<T> GetComponents<T>();
    }

    interface IComponentOwner<out T> : IComponentOwner where T : IComponentOwner<T>
    {
        new IEnumerable<TComponent> GetComponents<TComponent>() where TComponent : IComponent<T>;
    }
}
