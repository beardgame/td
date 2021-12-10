using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;

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