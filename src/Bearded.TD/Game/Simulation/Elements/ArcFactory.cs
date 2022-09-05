using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Weapons;

namespace Bearded.TD.Game.Simulation.Elements;

static class ArcFactory
{
    public static GameObject CreateArc(
        IComponentOwnerBlueprint blueprint, GameObject parent, GameObject target, UntypedDamage damage)
    {
        var obj = GameObjectFactory.CreateFromBlueprintWithDefaultRenderer(blueprint, parent, parent.Position);

        obj.AddComponent(new Property<Target>(target.AsTarget()));
        obj.AddComponent(new Property<UntypedDamage>(damage));
        obj.AddComponent(new HitTargetOnActivate());

        return obj;
    }
}
