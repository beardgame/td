using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Gameplay;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings
{
    [ComponentOwner]
    sealed class Building :
        BuildingBase<Building>,
        IIdable<Building>,
        IMortal,
        INamed
    {
        public Id<Building> Id { get; }
        public string Name { get; }

        private IBuildingState? state;
        private bool isDead;

        public Building(Id<Building> id, IBuildingBlueprint blueprint)
            : base(blueprint)
        {
            Id = id;
            Name = blueprint.Name;
        }

        protected override IEnumerable<IComponent<Building>> InitializeComponents()
            => Blueprint.GetComponentsForBuilding();

        protected override void OnAdded()
        {
            Game.IdAs(this);
            base.OnAdded();
        }

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
