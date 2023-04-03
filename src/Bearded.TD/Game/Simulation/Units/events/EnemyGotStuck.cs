using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Units;

readonly record struct EnemyGotStuck(Tile IntendedTarget) : IComponentEvent;
