using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Rendering;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons
{
    abstract class WeaponCycleHandler<TParameters> : Component<Weapon, TParameters>
        where TParameters : IParametersTemplate<TParameters>
    {
        private IWeaponTrigger? trigger;
        protected Weapon Weapon => Owner;
        protected GameState Game => Owner.Owner.Game;

        protected WeaponCycleHandler(TParameters parameters) : base(parameters)
        {
        }

        protected override void OnAdded()
        {
            ComponentDependencies.Depend<IWeaponTrigger>(Owner, Events, c => trigger = c);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (trigger?.TriggerPulled ?? false)
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

        public override void Draw(CoreDrawers drawers)
        {
        }
    }
}
