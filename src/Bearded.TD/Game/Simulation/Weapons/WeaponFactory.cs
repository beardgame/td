using Bearded.TD.Game.Simulation.Components;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

static class WeaponFactory
{
    public static ComponentGameObject Create(GameState game, ITurret turret, IComponentOwnerBlueprint blueprint)
    {
        var obj = new ComponentGameObject((IComponentOwner)turret.Owner, new Position3(), new Direction2());
        game.Add(obj);
        obj.AddComponent(new WeaponState(turret));
        foreach (var component in blueprint.GetComponents<ComponentGameObject>())
        {
            obj.AddComponent(component);
        }

        return obj;
    }
}
