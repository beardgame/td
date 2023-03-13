using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Projectiles;

readonly record struct HitLevel(Impact Info) : IComponentEvent;
