using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Components;

static class ComponentGameObjectFactory
{
    public static ComponentGameObject CreateFromBlueprintWithDefaultRenderer(
        GameState game,
        IComponentOwnerBlueprint blueprint,
        IComponentOwner? parent,
        Position3 position,
        Direction2? direction = null)
    {
        var obj = CreateFromBlueprintWithoutRenderer(game, blueprint, parent, position, direction);
        addDefaultRenderer(obj);
        return obj;
    }

    public static ComponentGameObject CreateFromBlueprintWithoutRenderer(
        GameState game,
        IComponentOwnerBlueprint blueprint,
        IComponentOwner? parent,
        Position3 position,
        Direction2? direction = null)
    {
        var obj = CreateWithoutRenderer(game, parent, position, direction);
        blueprint.GetComponents().ForEach(obj.AddComponent);
        return obj;
    }

    public static ComponentGameObject CreateWithDefaultRenderer(
        GameState game, IComponentOwner? parent, Position3 position, Direction2? direction = null)
    {
        var obj = CreateWithoutRenderer(game, parent, position, direction);
        addDefaultRenderer(obj);
        return obj;
    }

    public static ComponentGameObject CreateWithoutRenderer(
        GameState game, IComponentOwner? parent, Position3 position, Direction2? direction = null)
    {
        var obj = new ComponentGameObject(parent, position, direction ?? Direction2.Zero);
        game.Add(obj);
        return obj;
    }

    private static void addDefaultRenderer(ComponentGameObject obj)
    {
        obj.AddComponent(new DefaultComponentRenderer());
    }
}
