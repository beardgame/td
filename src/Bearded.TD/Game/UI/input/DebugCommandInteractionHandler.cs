using System;
using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Tiles;

namespace Bearded.TD.Game.UI
{
    class DebugCommandInteractionHandler : InteractionHandler
    {
        private readonly Func<PositionedFootprint, IRequest> requestFactory;
        protected override TileSelection TileSelection { get; }

        public DebugCommandInteractionHandler(GameInstance game, Func<PositionedFootprint, IRequest> requestFactory)
            : this(game, requestFactory, TileSelection.Single) { }

        public DebugCommandInteractionHandler(
            GameInstance game, Func<PositionedFootprint, IRequest> requestFactory, TileSelection tileSelection)
            : base(game)
        {
            this.requestFactory = requestFactory;
            TileSelection = tileSelection;
        }

        public override void Update(UpdateEventArgs args, ICursorHandler cursor)
        {
            if (cursor.ClickAction.Hit)
                Game.Request(requestFactory(cursor.CurrentFootprint));
        }
    }
}
