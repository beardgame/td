using amulware.Graphics;
using amulware.Graphics.Shapes;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Workers
{
    sealed class MiningTaskPlaceholder : GameObject, ISelectable
    {
        private readonly Faction faction;
        private readonly Tile tile;
        private readonly Id<IWorkerTask> taskId;
        private MiningTask task;

        public SelectionState SelectionState { get; private set; }

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
            Game.MiningLayer.MarkTileForMining(tile);
        }

        protected override void OnDelete()
        {
            Game.MiningLayer.CancelTileForMining(tile);

            base.OnDelete();
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (!task.Finished) return;

            Game.Meta.Events.Send(new TileMined(faction, tile));
            Delete();
        }

        public override void Draw(GeometryManager geometries)
        {
            var color = .5f * (SelectionState == SelectionState.Focused ? Color.DarkViolet : Color.MediumVioletRed);
            geometries.ConsoleBackground.FillCircle(
                Level.GetPosition(tile).NumericValue, Constants.Game.World.HexagonSide, color, 6);
        }

        public void ResetSelection()
        {
            SelectionState = SelectionState.Default;
        }

        public void Focus(SelectionManager selectionManager)
        {
            SelectionState = SelectionState.Focused;
        }

        public void Select(SelectionManager selectionManager) {}
    }
}
