using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

static class WeaponFactory
{
    public static GameObject Create(ITurret turret, IComponentOwnerBlueprint blueprint)
    {
        var obj = new GameObject(turret.Owner, new Position3(), new Direction2());
        obj.AddComponent(new WeaponState(turret));
        foreach (var component in blueprint.GetComponents())
        {
            obj.AddComponent(component);
        }

        return obj;
    }
}
