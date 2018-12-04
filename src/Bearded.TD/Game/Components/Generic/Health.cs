using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Commands;
using Bearded.TD.Meta;
using Bearded.TD.Mods.Models;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.Generic
{
    [Component("health")]
    class Health : Component<Building, IHealthComponentParameter>
    {
        public int CurrentHealth { get; private set; }
        public int MaxHealth => Parameters.MaxHealth;
        public double HealthPercentage => (double) CurrentHealth / MaxHealth;
        
        public Health(IHealthComponentParameter parameters) : base(parameters)
        {
            CurrentHealth = 1;
        }

        protected override void Initialise()
        {
            Owner.Damaged += onDamaged;
            Owner.HealthAdded += onHealthAdded;
        }

        private void onDamaged(int damage)
        {
            if (UserSettings.Instance.Debug.InvulnerableBuildings)
                return;

            changeHealth(-damage);
        }

        private void onHealthAdded(int health)
        {
            changeHealth(health);
        }

        private void changeHealth(int healthChange)
        {
            CurrentHealth = (CurrentHealth + healthChange).Clamped(0, MaxHealth);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (CurrentHealth <= 0)
            {
                Owner.Sync(KillBuilding.Command);
            }
        }

        public override void Draw(GeometryManager geometries) { }
    }
}
