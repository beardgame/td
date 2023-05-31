using System;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Elements;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Game.Simulation.Synchronization;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using static Bearded.Utilities.MathConstants;

namespace Bearded.TD.Game.Simulation.Units;

static class EnemyUnitFactory
{
    public static GameObject Create(Id<GameObject> id, IGameObjectBlueprint blueprint, Tile tile)
    {
        var unit = GameObjectFactory.CreateFromBlueprintWithDefaultRenderer(
            blueprint, null, Level.GetPosition(tile).WithZ(0));

        if (!unit.TryGetSingleComponent<IHealth>(out var health))
        {
            throw new InvalidOperationException("Enemies must have health component");
        }
        var radius =
            ((MathF.Atan(.005f * (health.MaxHealth.NumericValue - 200)) + PiOver2) / Pi * 0.6f).U();
        unit.AddComponent(new CapsuleCollider(new CapsuleColliderParametersTemplate(radius, radius * 2, true)));

        unit.AddComponent(new DamageAttributor());
        unit.AddComponent(new DamageSource());
        unit.AddComponent(new ElementSystemEntity());
        unit.AddComponent(new EnemyLife());
        unit.AddComponent(new HealthBar());
        unit.AddComponent(new HealthEventReceiver());
        unit.AddComponent(new IdProvider(id));
        unit.AddComponent(new Killable());
        unit.AddComponent(new EventReceiver<TakeHit>());

        unit.AddComponent(new Syncer());
        unit.AddComponent(new SyncPositionAndVelocity());

        unit.AddComponent(new TileBasedVisibility());
        unit.AddComponent(new CurrentTileNotifier());
        unit.AddComponent(new Targetable());
        unit.AddComponent(new PhysicalTilePresence());

        return unit;
    }
}
