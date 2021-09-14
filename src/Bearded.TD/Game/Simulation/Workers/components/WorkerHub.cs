using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Rendering;
using Bearded.Utilities.Geometry;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Workers
{
    [Component("workerHub")]
    sealed class WorkerHub<T> : Component<T, IWorkerHubParameters>
        where T : IComponentOwner, IGameObject, IPositionable
    {
        private Faction? faction;
        private int numWorkersActive;

        public WorkerHub(IWorkerHubParameters parameters) : base(parameters) { }

        protected override void Initialize()
        {
            ComponentDependencies.Depend<IOwnedByFaction>(Owner, Events, o => faction = o.Faction);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (Owner is IBuilding building && !building.State.IsFunctional)
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
            var obj = new ComponentGameObject(Parameters.Drone, Owner, Owner.Position, Direction2.Zero);
            if (faction != null)
            {
                obj.AddComponent(new OwnedByFaction<ComponentGameObject>(faction));
            }

            Owner.Game.Add(obj);
            numWorkersActive++;
        }
    }
}
