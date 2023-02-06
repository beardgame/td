using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Damage;

namespace Bearded.TD.Game.Simulation.Enemies;

interface IResistanceContributions
{
    ImmutableDictionary<SocketShape, Resistance> Factors { get; }
}
