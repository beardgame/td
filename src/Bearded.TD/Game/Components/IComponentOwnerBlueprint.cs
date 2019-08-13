using System.Collections.Generic;

namespace Bearded.TD.Game.Components
{
    interface IComponentOwnerBlueprint : IBlueprint
    {
        IEnumerable<IComponent<TOwner>> GetComponents<TOwner>();
    }
}
