using System.Linq;
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
            //for (var i = 0; i < Parameters.NumWorkers; i++)
            //{
            //    Owner.Game.Add(new Worker(Owner.Faction.Workers, Owner.Faction));
            //}

            var playerFactions = Owner.Game.Factions.Where(f => f.Parent == Owner.Faction).ToList();

            if (playerFactions.Count > 0)
            {
                foreach (var f in playerFactions)
                {
                    for (var i = 0; i < Parameters.NumWorkers / playerFactions.Count; i++)
                    {
                        Owner.Game.Add(new Worker(f.Workers, f));
                    }
                }
            }

            var leftOverWorkers = playerFactions.Count > 0
                ? Parameters.NumWorkers % playerFactions.Count
                : Parameters.NumWorkers;

            for (var i = 0; i < leftOverWorkers; i++)
            {
                Owner.Game.Add(new Worker(Owner.Faction.Workers, Owner.Faction));
            }
        }

        public override void Update(TimeSpan elapsedTime) { }
        public override void Draw(GeometryManager geometries) { }

    }
}
