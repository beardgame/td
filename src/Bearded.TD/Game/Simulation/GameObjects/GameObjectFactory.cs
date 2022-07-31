using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameObjects;

static class GameObjectFactory
{
    public static GameObject CreateFromBlueprintWithDefaultRenderer(
        IComponentOwnerBlueprint blueprint,
        GameObject? parent,
        Position3 position,
        Direction2? direction = null)
    {
        var obj = CreateFromBlueprintWithoutRenderer(blueprint, parent, position, direction);
        addDefaultRenderer(obj);
        return obj;
    }

    public static GameObject CreateFromBlueprintWithoutRenderer(
        IComponentOwnerBlueprint blueprint,
        GameObject? parent,
        Position3 position,
        Direction2? direction = null)
    {
        var obj = CreateWithoutRenderer(parent, position, direction);
        blueprint.GetComponents().ForEach(obj.AddComponent);
        return obj;
    }

    public static GameObject CreateWithDefaultRenderer(
        GameObject? parent, Position3 position, Direction2? direction = null)
    {
        var obj = CreateWithoutRenderer(parent, position, direction);
        addDefaultRenderer(obj);
        return obj;
    }

    public static GameObject CreateWithoutRenderer(
        GameObject? parent, Position3 position, Direction2? direction = null)
    {
        var obj = new GameObject(parent, position, direction ?? Direction2.Zero);
        obj.AddComponent(new GameObjectTags());
        return obj;
    }

    private static void addDefaultRenderer(GameObject obj)
    {
        obj.AddComponent(new DefaultComponentRenderer());
    }
}
