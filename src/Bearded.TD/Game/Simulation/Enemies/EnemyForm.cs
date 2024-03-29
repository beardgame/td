using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Enemies;

sealed record EnemyForm(
    IGameObjectBlueprint Blueprint,
    ImmutableDictionary<SocketShape, IModule> Modules,
    ImmutableDictionary<DamageType, Resistance> Resistances);
