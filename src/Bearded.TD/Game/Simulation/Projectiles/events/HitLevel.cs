using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Projectiles;

readonly record struct HitLevel(Impact Info, Tile Tile) : IComponentEvent;
