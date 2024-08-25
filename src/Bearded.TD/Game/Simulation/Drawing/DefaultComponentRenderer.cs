using System;
using Bearded.Graphics.Vertices;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.Loading;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing;

readonly record struct DrawComponents(CoreDrawers Core, IComponentDrawer Drawer) : IComponentEvent;

class DefaultComponentRenderer : Component, IComponentDrawer, IRenderable, IListener<ObjectDeleting>
{
    private IVisibility? visibility;

    public bool Deleted { get; private set; }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IVisibility>(Owner, Events, v => visibility = v);

        Events.Subscribe(this);
    }

    public override void Activate()
    {
        base.Activate();
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
        SpriteDrawInfo<TVertex, TVertexData> sprite, Vector3 position, float size, Angle angle, TVertexData data)
        where TVertex : struct, IVertexData
    {
        Drawable(sprite).Draw(SpriteLayout.CenteredAt(position, size, angle), data);
    }

    public void DrawQuad<TVertex, TVertexData>(
        SpriteDrawInfo<TVertex, TVertexData> sprite, Vector3 topLeft, Vector3 topRight, Vector3 bottomRight,
        Vector3 bottomLeft, TVertexData data)
        where TVertex : struct, IVertexData
    {
        Drawable(sprite).DrawQuad(topLeft, topRight, bottomRight, bottomLeft, data);
    }

    public void DrawIndexedVertices<TVertex, TVertexData>(
        SpriteDrawInfo<TVertex, TVertexData> sprite, int vertexCount, int indexCount, out Span<TVertex> vertices,
        out Span<ushort> indices, out ushort indexOffset, out UVRectangle uvs) where TVertex : struct, IVertexData
    {
        Drawable(sprite).DrawIndexedVertices(vertexCount, indexCount, out vertices, out indices, out indexOffset, out uvs);
    }

    public void DrawMesh(MeshDrawInfo mesh, Position3 position, Direction2 direction, Unit scale)
    {
        Drawable(mesh).Add(position.NumericValue, direction - Direction2.Zero, scale.NumericValue);
    }

    protected virtual DrawableMesh Drawable(MeshDrawInfo mesh)
    {
        return mesh.Mesh.AsDrawable(
            Owner.Game.Meta.DrawableRenderers,
            mesh.DrawGroup,
            mesh.DrawGroupOrderKey,
            mesh.Shader
            );
    }

    protected virtual IDrawableSprite<TVertex, TVertexData> Drawable<TVertex, TVertexData>(
        SpriteDrawInfo<TVertex, TVertexData> sprite)
        where TVertex : struct, IVertexData
    {
        return sprite.Sprite.MakeConcreteWith(
            Owner.Game.Meta.DrawableRenderers,
            sprite.DrawGroup, sprite.DrawGroupOrderKey,
            sprite.Create, sprite.Shader);
    }
}
