using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Buildings;

[Component("sink")]
sealed class EnemySink<T> : EnemySinkBase<T>
    where T : IComponentOwner, IGameObject, IPositionable
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