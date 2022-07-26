using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameObjects;

static class GameObjectFactory
{
    public static GameObject CreateFromBlueprintWithDefaultRenderer(
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

    public static GameObject CreateFromBlueprintWithoutRenderer(
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

    public static GameObject CreateWithDefaultRenderer(
        GameState game, IComponentOwner? parent, Position3 position, Direction2? direction = null)
    {
        var obj = CreateWithoutRenderer(game, parent, position, direction);
        addDefaultRenderer(obj);
        return obj;
    }

    public static GameObject CreateWithoutRenderer(
        GameState game, IComponentOwner? parent, Position3 position, Direction2? direction = null)
    {
        var obj = new GameObject(parent, position, direction ?? Direction2.Zero);
        obj.AddComponent(new GameObjectTags());
        game.Add(obj);
        return obj;
    }

    private static void addDefaultRenderer(GameObject obj)
    {
        obj.AddComponent(new DefaultComponentRenderer());
    }
}
