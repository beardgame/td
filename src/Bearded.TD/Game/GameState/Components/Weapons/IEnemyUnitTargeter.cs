using Bearded.TD.Game.GameState.Units;

namespace Bearded.TD.Game.GameState.Components.Weapons
{
    interface IEnemyUnitTargeter
    {
        EnemyUnit Target { get; }
    }
}
