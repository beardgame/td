using System.Collections.Immutable;
using Bearded.TD.Game.GameLoop;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.Units;
using static Bearded.TD.Testing.UniqueIds;
using GameObjectBlueprint = Bearded.TD.Content.Models.GameObjectBlueprint;
using IComponent = Bearded.TD.Content.Serialization.Models.IComponent;

namespace Bearded.TD.Testing.Enemies;

static class EnemyTestFactory
{
    public static ISpawnableEnemy CreateSpawnableEnemyWithUniqueSocket(out SocketShape socketShape)
    {
        socketShape = SocketShape.FromLiteral(NextUniquePrefixedString<SocketShape>("shape"));
        return CreateSpawnableEnemy(socketShape);
    }

    public static ISpawnableEnemy CreateSpawnableEnemy(params SocketShape[] socketShapes)
    {
        return new ScheduleGame.SpawnableEnemy(
            new GameObjectBlueprint(
                NextUniqueModAwareId(),
                socketShapes.Select(socketComponent)
                    .Append(healthComponent())
                    .Append(threatComponent(10))
                    .ToImmutableArray()), 1);
    }

    private static IComponent healthComponent()
    {
        return new Content.Serialization.Models.Component<Health.IParameters>
        {
            Id = "health",
            Parameters = new HealthParametersTemplate(null, null)
        };
    }

    private static IComponent threatComponent(float threat)
    {
        return new Content.Serialization.Models.Component<Threat.IParameters>
        {
            Id = "threat",
            Parameters = new ThreatParametersTemplate(threat)
        };
    }

    private static IComponent socketComponent(SocketShape socketShape)
    {
        return new Content.Serialization.Models.Component<Socket.IParameters>
        {
            Id = "socket",
            Parameters = new SocketParametersTemplate(socketShape)
        };
    }
}
