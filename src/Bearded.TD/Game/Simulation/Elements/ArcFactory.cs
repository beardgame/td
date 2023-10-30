using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Projectiles;

namespace Bearded.TD.Game.Simulation.Elements;

static class ArcFactory
{
    public static GameObject CreateArc(
        IGameObjectBlueprint blueprint,
        GameObject parent,
        GameObject source,
        GameObject target,
        UntypedDamage damage)
    {
        var obj = GameObjectFactory.CreateFromBlueprintWithDefaultRenderer(blueprint, parent, source.Position);

        obj.AddComponent(new Property<Source>(source.AsSource()));
        obj.AddComponent(new Property<Target>(target.AsTarget()));
        obj.AddComponent(new Property<UntypedDamage>(damage));
        obj.AddComponent(new HitTargetOnActivate());

        return obj;
    }
}
