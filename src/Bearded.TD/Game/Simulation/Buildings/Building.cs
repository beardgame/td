using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Gameplay;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Rendering;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings
{
    [ComponentOwner]
    sealed class Building :
        BuildingBase<Building>,
        IMortal
    {
        private IBuildingState? state;
        private bool isDead;

        public Building(IBuildingBlueprint blueprint) : base(blueprint) {}

        protected override IEnumerable<IComponent<Building>> InitializeComponents()
            => Blueprint.GetComponentsForBuilding();

        public void OnDeath()
        {
            isDead = true;
        }

        protected override void OnDelete()
        {
            Game.BuildingLayer.RemoveBuilding(this);
            base.OnDelete();
        }

        public override void Update(TimeSpan elapsedTime)
        {
            base.Update(elapsedTime);

            state ??= GetComponents<IBuildingStateProvider>().SingleOrDefault()?.State;
            if (isDead)
            {
                this.Sync(KillBuilding.Command);
            }
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
