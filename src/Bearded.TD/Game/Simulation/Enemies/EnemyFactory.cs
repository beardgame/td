using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Tiles;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Enemies;

static class EnemyFactory
{
    public static GameObject Create(Id<GameObject> id, EnemyForm form, Tile tile)
    {
        var obj = EnemyUnitFactory.Create(id, form.Blueprint, tile); // TODO: move the method to this class
        fillSockets(obj, form.Modules);
        obj.AddComponent(new DamageResistances(form.Resistances));
        return obj;
    }

    private static void fillSockets(GameObject obj, ImmutableDictionary<SocketShape,IModule> modules)
    {
        foreach (var socket in obj.GetComponents<ISocket>())
        {
            if (modules.TryGetValue(socket.Shape, out var module))
            {
                socket.Fill(module);
            }
        }
    }
}
