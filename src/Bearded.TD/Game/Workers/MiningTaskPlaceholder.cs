using amulware.Graphics;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Meta;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Workers
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
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (task.Finished)
                Delete();
        }

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.ConsoleBackground;
            var color = SelectionState == SelectionState.Focused ? Color.DarkViolet : Color.MediumVioletRed;
            geo.Color = color * 0.5f;
            geo.DrawCircle(Level.GetPosition(tile).NumericValue, Constants.Game.World.HexagonSide, true, 6);
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
