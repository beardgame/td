using Bearded.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.Vertices;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

sealed class IconStatusDrawer : IStatusDrawer
{
    private readonly SpriteDrawInfo<UVColorVertex, Color> sprite;

    private IconStatusDrawer(SpriteDrawInfo<UVColorVertex, Color> sprite)
    {
        this.sprite = sprite;
    }

    public void Draw(CoreDrawers core, IComponentDrawer drawer, Vector3 position, float size)
    {
        drawer.DrawSprite(sprite, position, .85f * size, 0f, Color.White);
    }

    public static IconStatusDrawer FromSpriteBlueprint(GameState game, ISpriteBlueprint blueprint) =>
        new(SpriteDrawInfo.ForUVColor(game, blueprint, drawGroup: SpriteDrawGroup.IgnoreDepth));
}
