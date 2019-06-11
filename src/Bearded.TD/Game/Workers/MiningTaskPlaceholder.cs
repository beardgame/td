using amulware.Graphics;
using Bearded.TD.Game.Factions;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Workers
{
    sealed class MiningTaskPlaceholder : GameObject
    {
        private readonly Faction faction;
        private readonly Tile tile;
        private readonly Id<IWorkerTask> taskId;
        private MiningTask task;

        public MiningTaskPlaceholder(Faction faction, Tile tile, Id<IWorkerTask> taskId)
        {
            this.faction = faction;
            this.tile = tile;

            this.taskId = taskId;
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            task = new MiningTask(taskId, this, tile, Game.GeometryLayer);
            faction.Workers.RegisterTask(task);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (task.Finished)
                Delete();
        }

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.ConsoleBackground;
            geo.Color = Color.MediumVioletRed * 0.5f;
            geo.DrawCircle(Level.GetPosition(tile).NumericValue, Constants.Game.World.HexagonSide, true, 6);
        }
    }
}
