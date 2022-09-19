using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements;

static class ArcFactory
{
    public static GameObject CreateArc(
        IGameObjectBlueprint blueprint,
        GameObject parent,
        GameObject target,
        UntypedDamage damage,
        TimeSpan lifeTime)
    {
        var obj = GameObjectFactory.CreateFromBlueprintWithDefaultRenderer(blueprint, parent, parent.Position);

        obj.AddComponent(new Property<Target>(target.AsTarget()));
        obj.AddComponent(new Property<UntypedDamage>(damage));
        obj.AddComponent(new HitTargetOnActivate());
        obj.AddComponent(new DeleteAfter(new DeleteAfterParametersTemplate(lifeTime)));

        return obj;
    }
}
