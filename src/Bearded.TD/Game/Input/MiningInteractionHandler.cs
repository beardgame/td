using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Factions;

namespace Bearded.TD.Game.Input
{
    class MiningInteractionHandler : InteractionHandler
    {
        private readonly Faction faction;

        public MiningInteractionHandler(GameInstance game, Faction faction) : base(game)
        {
            this.faction = faction;
        }

        public override void Update(UpdateEventArgs args, ICursorHandler cursor)
        {
            var currentTile = cursor.CurrentFootprint;
            if (!currentTile.IsValid)
                return;
            if (cursor.Click.Hit)
                foreach (var tile in currentTile.OccupiedTiles.Where(t => t.Info.IsMineable))
                    Game.RequestDispatcher.Dispatch(MineTile.Request(Game, faction, tile));
        }
    }
}
