using System;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Synchronization;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Units;

static class EnemyUnitFactory
{
    public static GameObject Create(
        GameState game, Id<GameObject> id, IComponentOwnerBlueprint blueprint, Tile tile)
    {
        var unit = ComponentGameObjectFactory.CreateFromBlueprintWithDefaultRenderer(
            game, blueprint, null, Level.GetPosition(tile).WithZ(0));

        if (!unit.TryGetSingleComponent<IHealth>(out var health))
        {
            throw new InvalidOperationException("Enemies must have health component");
        }
        var radius =
            ((MathF.Atan(.005f * (health.MaxHealth.NumericValue - 200)) + MathConstants.PiOver2) / MathConstants.Pi * 0.6f).U();
        unit.AddComponent(new CircleCollider(radius));

        unit.AddComponent(new EnemyLifeCycle());
        unit.AddComponent(new HealthBar());
        unit.AddComponent(new HealthEventReceiver());
        unit.AddComponent(new DamageSource());
        unit.AddComponent(new IdProvider(id));
        unit.AddComponent(new Syncer());
        unit.AddComponent(new TileBasedVisibility());
        return unit;
    }
}
