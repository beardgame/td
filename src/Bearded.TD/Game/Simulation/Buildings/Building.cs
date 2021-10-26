using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Rendering;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings
{
    [ComponentOwner]
    sealed class Building : BuildingBase<Building>
    {
        private IBuildingState? state;

        public Building(IBuildingBlueprint blueprint) : base(blueprint) {}

        protected override IEnumerable<IComponent<Building>> InitializeComponents()
            => Blueprint.GetComponentsForBuilding();

        public override void Update(TimeSpan elapsedTime)
        {
            base.Update(elapsedTime);

            state ??= GetComponents<IBuildingStateProvider>().SingleOrDefault()?.State;
        }

        public override void Draw(CoreDrawers drawers)
        {
            if (!state?.IsMaterialized ?? true)
            {
                return;
            }

            base.Draw(drawers);
        }
    }
}
