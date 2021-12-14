using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class BackupSink<T> : EnemySinkBase<T>
    where T : IComponentOwner, IGameObject
{
    protected override void AddSink(Tile t)
    {
        Owner.Game.Navigator.AddBackupSink(t);
    }

    protected override void RemoveSink(Tile t)
    {
        Owner.Game.Navigator.RemoveSink(t);
    }
}