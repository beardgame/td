using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components.Damage;
using Bearded.UI.Navigation;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    sealed class BuildingStatusOverlay : UpdateableNavigationNode<IPlacedBuilding>
    {
        public IPlacedBuilding Building { get; private set; } = null!;

        // TODO: invoke this event when the placeholder is replaced by the building instead of closing overlay
        public event VoidEventHandler? BuildingSet;
        public event VoidEventHandler? BuildingUpdated;

        public (int CurrentHealth, int MaxHealth)? BuildingHealth
        {
            get
            {
                if (!(Building is Building b))
                {
                    return null;
                }

                var health = b.GetComponents<Health<Building>>().FirstOrDefault();
                if (health == null)
                {
                    return null;
                }

                return (health.CurrentHealth, health.MaxHealth);
            }
        }

        protected override void Initialize(DependencyResolver dependencies, IPlacedBuilding building)
        {
            base.Initialize(dependencies, building);
            Building = building;
            Building.Deleting += Close;
        }

        public override void Update(UpdateEventArgs args)
        {
            BuildingUpdated?.Invoke();
        }

        public override void Terminate()
        {
            Building.Deleting -= Close;

            base.Terminate();
        }

        public void Close()
        {
            Navigation.Exit();
        }
    }
}
