using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Game.Simulation.Enemies;

sealed class EnemyFormGeneratorPoc
{
    // TODO: this should be loaded from the mod file (can be cached)
    private static readonly ILookup<SocketShape, IModule> modulesBySocket = null!;

    public static EnemyForm GenerateEnemyForm(IGameObjectBlueprint blueprint)
    {
        var instantiatedEnemy = EnemyUnitFactory.Create(Id<GameObject>.Invalid, blueprint, Tile.Origin);
        var sockets = instantiatedEnemy.GetComponents<ISocket>();
        // assumption: if you have multiple sockets of the same shape, they will all receive the same module
        // TODO: should sockets instead get an identifier so we can have multiple sockets of the same shape?
        var shapes = sockets.Select(s => s.Shape).Distinct();
        var modules = assignModulesToShapes(shapes);
        return new EnemyForm(blueprint, modules);
    }

    private static ImmutableDictionary<SocketShape, IModule> assignModulesToShapes(IEnumerable<SocketShape> shapes)
    {
        return shapes.ToImmutableDictionary(s => s, s => chooseAppropriateModule(modulesBySocket[s]));
    }

    private static IModule chooseAppropriateModule(IEnumerable<IModule> modules)
    {
        // TODO: this should actually consider primary or accent element
        return modules.RandomElement();
    }

    private static object deriveElementalResistances()
    {
        return null!;
    }
}
