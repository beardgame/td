using System;
using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Elements;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.Game.Simulation.Synchronization;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Enemies;

static class EnemyFactory
{
    public static GameObject Create(Id<GameObject> id, EnemyForm form, Tile tile)
    {
        var obj = createOrphanedFromBlueprint(form.Blueprint, tile);

        obj.AddComponent(new IdProvider(id));
        addGameplayComponents(obj);

        fillSockets(obj, form.Modules);
        obj.AddComponent(new DamageResistances(form.Resistances));

        return obj;
    }

    private static void addGameplayComponents(GameObject obj)
    {
        if (!obj.TryGetSingleComponent<IHealth>(out var health))
        {
            throw new InvalidOperationException("Enemies must have health component");
        }
        var radius =
            ((MathF.Atan(.005f * (health.MaxHealth.NumericValue - 200)) + MathConstants.PiOver2) / MathConstants.Pi * 0.6f).U();
        obj.AddComponent(new CapsuleCollider(new CapsuleColliderParametersTemplate(radius, radius * 2, true)));

        obj.AddComponent(new DamageAttributor());
        obj.AddComponent(new DamageSource());
        obj.AddComponent(new ElementSystemEntity());
        obj.AddComponent(new EnemyLife());
        obj.AddComponent(new HealthEventReceiver());
        obj.AddComponent(new Killable());
        obj.AddComponent(new StatusDisplay());
        obj.AddComponent(new EventReceiver<TakeHit>());

        obj.AddComponent(new Syncer());
        obj.AddComponent(new SyncPositionAndVelocity());

        obj.AddComponent(new TileBasedVisibility());
        obj.AddComponent(new CurrentTileNotifier());
        obj.AddComponent(new Targetable());
        obj.AddComponent(new PhysicalTilePresence());
    }

    private static void fillSockets(GameObject obj, ImmutableDictionary<SocketShape, IModule> modules)
    {
        foreach (var socket in obj.GetComponents<ISocket>().ToImmutableArray())
        {
            if (modules.TryGetValue(socket.Shape, out var module))
            {
                socket.Fill(module);
            }
        }
    }

    public static GameObject CreateTemplate(IGameObjectBlueprint blueprint)
    {
        var obj = createOrphanedFromBlueprint(blueprint, Tile.Origin);
        obj.AddComponent(new ThrowOnActivate()); // safety net to avoid this object from being added to the game
        return obj;
    }

    private static GameObject createOrphanedFromBlueprint(IGameObjectBlueprint blueprint, Tile tile)
    {
        return GameObjectFactory
            .CreateFromBlueprintWithDefaultRenderer(blueprint, null, Level.GetPosition(tile).WithZ(0));
    }
}
