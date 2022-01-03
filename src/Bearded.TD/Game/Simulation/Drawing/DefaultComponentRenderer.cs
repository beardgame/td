using Bearded.Graphics.Vertices;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Rendering;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Simulation.Drawing;

readonly record struct DrawComponents(CoreDrawers Core, IComponentDrawer Drawer) : IComponentEvent;

class DefaultComponentRenderer<T> : Component<T>, IComponentDrawer, IRenderable, IListener<ObjectDeleting>
    where T : IGameObject, IComponentOwner
{
    private IVisibility? visibility;

    public bool Deleted { get; private set; }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IVisibility>(Owner, Events, v => visibility = v);

        Events.Subscribe(this);
        Owner.Game.ListAs<IRenderable>(this);
    }

    public override void OnRemoved() => Deleted = true;
    public void HandleEvent(ObjectDeleting @event) => Deleted = true;

    public override void Update(TimeSpan elapsedTime)
    {
    }


    public void Render(CoreDrawers drawers)
    {
        if (visibility?.Visibility.IsVisible() == false)
            return;

        Events.Send(new DrawComponents(drawers, this));
    }

    public void DrawSprite<TVertex, TVertexData>(
        SpriteDrawInfo<TVertex, TVertexData> sprite, Vector3 position, float size, float angle, TVertexData data)
        where TVertex : struct, IVertexData
    {
        Drawable(sprite).Draw(position, size, angle, data);
    }

    public void DrawQuad<TVertex, TVertexData>(
        SpriteDrawInfo<TVertex, TVertexData> sprite, Vector3 topLeft, Vector3 topRight, Vector3 bottomRight,
        Vector3 bottomLeft, TVertexData data)
        where TVertex : struct, IVertexData
    {
        Drawable(sprite).DrawQuad(topLeft, topRight, bottomRight, bottomLeft, data);
    }

    public void DrawQuad<TVertex, TVertexData>(
        SpriteDrawInfo<TVertex, TVertexData> sprite,
        Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector2 uv0,
        Vector2 uv1, Vector2 uv2, Vector2 uv3, TVertexData data)
        where TVertex : struct, IVertexData
    {
        Drawable(sprite).DrawQuad(p0, p1, p2, p3, uv0, uv1, uv2, uv3, data);
    }

    protected virtual IDrawableSprite<TVertexData> Drawable<TVertex, TVertexData>(
        SpriteDrawInfo<TVertex, TVertexData> sprite)
        where TVertex : struct, IVertexData
    {
        return sprite.Sprite.MakeConcreteWith(
            Owner.Game.Meta.SpriteRenderers,
            sprite.DrawGroup, sprite.DrawGroupOrderKey,
            sprite.Create, sprite.Shader);
    }
}
