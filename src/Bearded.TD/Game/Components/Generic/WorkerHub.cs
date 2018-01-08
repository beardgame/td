using Bearded.TD.Game.Resources;
using Bearded.TD.Mods.Models;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.Generic
{
    class WorkerHub<T> : Component<T, WorkerHubParameters>
        where T : GameObject, IFactioned
    {
        private const int numWorkers = 2;

        public WorkerHub(WorkerHubParameters parameters) : base(parameters) { }

        protected override void Initialise()
        {
            for (var i = 0; i < numWorkers; i++)
            {
                Owner.Game.Add(new Worker(Owner.Faction.Workers, Owner.Faction));
            }
        }

        public override void Update(TimeSpan elapsedTime) { }
        public override void Draw(GeometryManager geometries) { }

    }
}
