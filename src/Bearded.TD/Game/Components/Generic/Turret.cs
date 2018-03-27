using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Weapons;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.Generic
{
    [Component("turret")]
    class Turret<T> : Component<T, TurretParameters>
        where T : BuildingBase<T>
    {
        private Weapon<T> weapon;

        public Turret(TurretParameters parameters) : base(parameters) { }

        protected override void Initialise()
        {
            weapon = new Weapon<T>(Parameters.Weapon);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            weapon.Update(elapsedTime);
        }
        
        public override void Draw(GeometryManager geometries)
        {
            weapon.Draw(geometries);
        }
    }
}