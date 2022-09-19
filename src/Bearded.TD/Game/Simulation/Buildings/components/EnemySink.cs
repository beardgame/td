using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Tiles;
using Bearded.Utilities.Collections;

namespace Bearded.TD.Game.Simulation.Buildings;

[Component("sink")]
sealed class EnemySink : EnemySinkBase
{
    private Target? target;

    protected override void AddSink(Tile t)
    {
        Owner.Game.Navigator.AddSink(t);
    }

    protected override void RemoveSink(Tile t)
    {
        Owner.Game.Navigator.RemoveSink(t);
    }

    protected override void Register()
    {
        base.Register();
        if (target != null) return;
        target = new Target(Level.GetTile(Owner.Position));
        Owner.Game.ListAs<ITarget>(target);
    }

    protected override void Unregister()
    {
        target?.Delete();
        target = null;
        base.Unregister();
    }

    public interface ITarget : IDeletable
    {
        public Tile Tile { get; }
    }

    private sealed class Target : ITarget
    {
        public Tile Tile { get; }
        public bool Deleted { get; private set; }

        public Target(Tile tile)
        {
            Tile = tile;
        }

        public void Delete()
        {
            Deleted = true;
        }
    }
}
