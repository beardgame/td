using Bearded.TD.Game;
using Bearded.TD.Game.Events;
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;

namespace Bearded.TD.Rendering.Deferred
{
    /* TODO: implement sprite->heightmap rendering
     *     - take 'wall' as default
     *     - render open areas (floors < or > crevices?)
     *     - use simple generated sprites for testing
     *     - add sprites to mod later, and experiment with what's possible
     * TODO: See if dedicated cliff rendering is necessary
     * TODO: once reasonably satisfied, move implementation to GPU
     *     - keep CPU renderer as alternative as long as possible
     * TODO: try tessellation on long triangles on GPU
     *     - could even replace some of the existing detail, to render less geometry if camera is far away
     * TODO: try using heightmaps for materials to apply vertex offsets in normal direction
     *     - might mess with normals, but could make great use of tessellation
     * TODO: ceilings (maybe GPU only?)
     *     - can probably be generated from the same heightmaps
     *     - stop rendering current wall-tops, replace by cross-section of rock
     *         - cross sections could later give hints at what's 'in' the walls?
     *     - try rendering front faces with regular level shaders, and back faces (separate pass) with special shader
     */

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
    }
}
