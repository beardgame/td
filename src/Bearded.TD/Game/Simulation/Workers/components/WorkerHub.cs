using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Rendering;
using Bearded.Utilities.Geometry;
using static Bearded.TD.Utilities.DebugAssert;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Workers
{
    [Component("workerHub")]
    sealed class WorkerHub<T> : Component<T, IWorkerHubParameters>
        where T : IComponentOwner, IGameObject, IPositionable
    {
        private IBuildingState? state;
        private Faction? faction;
        private int numWorkersActive;

        public WorkerHub(IWorkerHubParameters parameters) : base(parameters) { }

        protected override void Initialize()
        {
            ComponentDependencies.Depend<IBuildingStateProvider>(Owner, Events, p => state = p.State);
            ComponentDependencies.Depend<IOwnedByFaction>(Owner, Events, o => faction = o.Faction);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (faction == null || !(state?.IsFunctional ?? false))
            {
                return;
            }

            while (numWorkersActive < Parameters.NumWorkers)
            {
                addNewWorker();
            }
        }

        public override void Draw(CoreDrawers drawers) { }

        private void addNewWorker()
        {
            State.Satisfies(faction != null);
            var obj = new ComponentGameObject(Parameters.Drone, Owner, Owner.Position, Direction2.Zero);
            obj.AddComponent(new OwnedByFaction<ComponentGameObject>(faction!));

            Owner.Game.Add(obj);
            numWorkersActive++;
        }
    }
}
