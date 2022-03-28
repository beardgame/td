using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Buildings;

[Component("sink")]
sealed class EnemySink : EnemySinkBase
{
    protected override void AddSink(Tile t)
    {
        Owner.Game.Navigator.AddSink(t);
    }

    protected override void RemoveSink(Tile t)
    {
        Owner.Game.Navigator.RemoveSink(t);
    }
}
