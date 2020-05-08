using Bearded.TD.Game.Events;

namespace Bearded.TD.Game.Rules
{
    interface IGameRule<in TOwner>
    {
        void OnAdded(TOwner owner, GlobalGameEvents events);
    }
}
