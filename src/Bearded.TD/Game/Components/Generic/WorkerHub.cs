using Bearded.TD.Game.Resources;
using Bearded.TD.Mods.Models;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.Generic
{
    [Component("workerHub")]
    class WorkerHub<T> : Component<T, WorkerHubParameters>
        where T : GameObject, IFactioned
    {
        public WorkerHub(WorkerHubParameters parameters) : base(parameters) { }

        protected override void Initialise()
        {
            for (var i = 0; i < Parameters.NumWorkers; i++)
            {
                Owner.Game.Add(new Worker(Owner.Faction.Workers, Owner.Faction));
            }
        }

        public override void Update(TimeSpan elapsedTime) { }
        public override void Draw(GeometryManager geometries) { }

    }
}
