using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Synchronization;
using Bearded.TD.Tiles;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Units
{
    static class EnemyUnitFactory
    {
        public static EnemyUnit Create(GameState game, Id<EnemyUnit> id, IUnitBlueprint blueprint, Tile tile)
        {
            var unit = new EnemyUnit(blueprint, tile);
            game.Add(unit);
            unit.AddComponent(new DefaultComponentRenderer<EnemyUnit>());
            unit.AddComponent(new HealthEventReceiver<EnemyUnit>());
            unit.AddComponent(new DamageSource<EnemyUnit>());
            unit.AddComponent(new IdProvider<EnemyUnit>(id));
            unit.AddComponent(new Syncer<EnemyUnit>());
            unit.AddComponent(new TileBasedVisibility<EnemyUnit>());
            return unit;
        }
    }
}
