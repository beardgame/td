using Bearded.TD.Game.Weapons;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.Generic
{
    abstract class WeaponCycleHandler<TParameters> : IComponent<Weapon>
    {
        protected TParameters Parameters { get; }

        public Weapon Weapon { get; private set; }

        Weapon IComponent<Weapon>.Owner => Weapon;
        protected GameState Game => Weapon.Owner.Game;

        protected WeaponCycleHandler(TParameters parameters)
        {
            Parameters = parameters;
        }

        public void OnAdded(Weapon owner)
        {
            Weapon = owner;
            Initialize();
        }

        protected virtual void Initialize()
        {
        }

        public virtual void Update(TimeSpan elapsedTime)
        {
            if (Weapon.ShootingThisFrame)
            {
                UpdateShooting(elapsedTime);
            }
            else
            {
                UpdateIdle(elapsedTime);
            }
        }

        protected virtual void UpdateShooting(TimeSpan elapsedTime)
        {
        }

        protected virtual void UpdateIdle(TimeSpan elapsedTime)
        {
        }


        public virtual void Draw(GeometryManager geometries)
        {
        }
    }
}