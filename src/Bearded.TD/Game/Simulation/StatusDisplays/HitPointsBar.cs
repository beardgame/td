using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Damage;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

sealed record HitPointsBar(IHitPointsPool Pool, DamageShell Shell, Color Color);
