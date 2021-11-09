using Bearded.TD.Game.Simulation.Drawing;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Components
{
    static class ComponentGameObjectFactory
    {
        public static ComponentGameObject CreateWithDefaultRenderer(
            GameState game,
            IComponentOwnerBlueprint blueprint,
            IComponentOwner? parent,
            Position3 position,
            Direction2? direction = null)
        {
            var obj = CreateWithoutRenderer(game, blueprint, parent, position, direction);

            obj.AddComponent(new DefaultComponentRenderer<ComponentGameObject>());

            return obj;
        }

        public static ComponentGameObject CreateWithoutRenderer(
            GameState game,
            IComponentOwnerBlueprint blueprint,
            IComponentOwner? parent,
            Position3 position,
            Direction2? direction = null)
        {
            var obj = new ComponentGameObject(blueprint, parent, position, direction ?? Direction2.Zero);
            game.Add(obj);
            return obj;
        }
    }
}
