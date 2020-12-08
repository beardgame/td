using Bearded.TD.Game.GameState.Events;

namespace Bearded.TD.Game.GameState.Rules
{
    interface IGameRule<in TOwner>
    {
        void OnAdded(TOwner owner, GlobalGameEvents events);
    }
}
