using System.Collections.Immutable;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.Model;
using Bearded.TD.Game.Simulation.Upgrades;
using static Bearded.TD.Testing.UniqueIds;

namespace Bearded.TD.Testing.Enemies;

static class ModuleTestFactory
{
    public static IModule CreateModule(SocketShape socketShape, Element element)
    {
        return new Module(
            NextUniqueModAwareId(prefix: "module"), element, socketShape, ImmutableArray<IUpgradeEffect>.Empty);
    }

    public static ImmutableArray<IModule> CreateModulesForAllElements(SocketShape socketShape)
    {
        return ElementExtensions.Enumerate().Select(e => CreateModule(socketShape, e)).ToImmutableArray();
    }

    public static ISocket CreateSocket(out SocketShape socketShape)
    {
        socketShape = SocketShape.FromLiteral(NextUniquePrefixedString<SocketShape>("shape"));
        return CreateSocket(socketShape);
    }

    public static ISocket CreateSocket(SocketShape socketShape)
    {
        return new Socket(new SocketParametersTemplate(socketShape));
    }
}
