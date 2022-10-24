using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.GameLoop;

interface ISpawnableEnemy : IBlueprint
{
    IGameObjectBlueprint Blueprint { get; }
    double Probability { get; }
}
