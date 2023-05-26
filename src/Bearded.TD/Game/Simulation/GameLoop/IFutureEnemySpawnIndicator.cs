using Bearded.TD.Game.Simulation.Enemies;

namespace Bearded.TD.Game.Simulation.GameLoop;

interface IFutureEnemySpawnIndicator
{
    void AddFutureEnemySpawn(EnemyForm form, int amount);
    void FulfilFutureEnemySpawn(EnemyForm form);
}
