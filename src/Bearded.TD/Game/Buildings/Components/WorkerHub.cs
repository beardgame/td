using Bearded.TD.Game.Resources;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Buildings.Components
{
    class WorkerHub : Component
    {
        private const int numWorkers = 2;
        
        protected override void Initialise()
        {
            for (var i = 0; i < numWorkers; i++)
                Building.Game.Add(new Worker(Building.Game.WorkerManager, Building.Faction));
        }

        public override void Update(TimeSpan elapsedTime) { }
        public override void Draw(GeometryManager geometries) { }
    }
}
