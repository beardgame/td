using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Shared.Events;

namespace Bearded.TD.Game.Simulation.Modules;

[Component("dieOnStuck")]
sealed partial class DieOnStuck : Component
{
    [Handler]
    private void onEnemyGotStuck(EnemyGotStuck e)
    {
        Events.Send(new EnactDeath());
    }
}
