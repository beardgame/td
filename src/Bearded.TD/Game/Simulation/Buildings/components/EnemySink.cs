using System;
using System.Collections.Generic;
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
        target = new Target(Level.GetTile(Owner.Position), () => OccupiedTiles);
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
        Tile Tile { get; }
        IEnumerable<Tile> AllOccupiedTiles { get; }
    }

    private sealed class Target : ITarget
    {
        private readonly Func<IEnumerable<Tile>> occupiedTilesGetter;

        public Tile Tile { get; }
        public bool Deleted { get; private set; }

        public IEnumerable<Tile> AllOccupiedTiles => occupiedTilesGetter();

        public Target(Tile tile, Func<IEnumerable<Tile>> occupiedTilesGetter)
        {
            Tile = tile;
            this.occupiedTilesGetter = occupiedTilesGetter;
        }

        public void Delete()
        {
            Deleted = true;
        }
    }
}
