using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Rules
{
    interface IGameRule<in TOwner>
    {
        void OnAdded(TOwner owner, GlobalGameEvents events);
    }
}
