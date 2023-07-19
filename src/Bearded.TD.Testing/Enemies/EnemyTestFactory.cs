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
    public static ISpawnableEnemy CreateSpawnableEnemyWithUniqueSocket(Archetype archetype, out SocketShape socketShape)
    {
        socketShape = SocketShape.FromLiteral(NextUniquePrefixedString<SocketShape>("shape"));
        return CreateSpawnableEnemy(archetype, socketShape);
    }

    public static ISpawnableEnemy CreateSpawnableEnemy(Archetype archetype, params SocketShape[] socketShapes)
    {
        return new ScheduleGame.SpawnableEnemy(
            new GameObjectBlueprint(
                NextUniqueModAwareId(),
                socketShapes.Select(socketComponent)
                    .Append(healthComponent())
                    .Append(threatComponent(10))
                    .Append(archetypeComponent(archetype))
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

    private static IComponent archetypeComponent(Archetype archetype)
    {
        return new Content.Serialization.Models.Component<ArchetypeProperty.IParameters>
        {
            Id = "archetype",
            Parameters = new ArchetypePropertyParametersTemplate(archetype)
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
