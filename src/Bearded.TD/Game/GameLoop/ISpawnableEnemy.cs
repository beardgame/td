using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.GameLoop;

interface ISpawnableEnemy
{
    IGameObjectBlueprint Blueprint { get; }
    double Probability { get; }
}
