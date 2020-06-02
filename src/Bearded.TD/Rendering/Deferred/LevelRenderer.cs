using Bearded.TD.Game;
using Bearded.TD.Game.Events;
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;

namespace Bearded.TD.Rendering.Deferred
{
    abstract class LevelRenderer : IListener<TileDrawInfoChanged>
    {
        protected LevelRenderer(GameInstance game)
        {
            game.Meta.Events.Subscribe(this);
        }

        public void HandleEvent(TileDrawInfoChanged @event)
        {
            OnTileChanged(@event.Tile);
        }

        protected abstract void OnTileChanged(Tile tile);
        public abstract void RenderAll();
        public abstract void CleanUp();

        public virtual void PrepareForRender()
        {
        }
    }
}
