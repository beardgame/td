using Bearded.TD.Game.GameState.Weapons;
using Bearded.TD.Rendering;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.GameState.Components.Weapons
{
    abstract class WeaponCycleHandler<TParameters> : Component<Weapon, TParameters>
        where TParameters : IParametersTemplate<TParameters>
    {
        protected Weapon Weapon => Owner;
        protected GameState Game => Owner.Owner.Game;

        protected WeaponCycleHandler(TParameters parameters) : base(parameters)
        {
        }

        protected override void Initialise()
        {
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (Owner.ShootingThisFrame)
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

        public override void Draw(GeometryManager geometries)
        {
        }
    }
}
