using amulware.Graphics;
using Bearded.TD.Game.Factions;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Workers
{
    sealed class MiningTaskPlaceholder : GameObject, IIdable<MiningTaskPlaceholder>
    {
        public Id<MiningTaskPlaceholder> Id { get; }
        private readonly Faction faction;
        private readonly Tile tile;
        private MiningTask task;

        public MiningTaskPlaceholder(Id<MiningTaskPlaceholder> id, Faction faction, Tile tile)
        {
            Id = id;
            this.faction = faction;
            this.tile = tile;
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            Game.IdAs(this);

            task = new MiningTask(this, tile, Game.GeometryLayer);
            faction.Workers.RegisterTask(task);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (task.Finished)
                Delete();
        }

        public void Cancel()
        {
            faction.Workers.AbortTask(task);
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
