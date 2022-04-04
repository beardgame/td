using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Rendering;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Drawing;

sealed class GhostBuildingRenderer : DefaultComponentRenderer, IListener<ConstructionStarted>
{
    private Shader shader = null!;
    private bool switchToDefaultRenderer;

    protected override void OnAdded()
    {
        base.OnAdded();

        shader = Owner.Game.Meta.Blueprints.Shaders[ModAwareId.ForDefaultMod("building-ghost")];
        Events.Subscribe<ConstructionStarted>(this);
    }

    public override void OnRemoved()
    {
        base.OnRemoved();

        Events.Unsubscribe<ConstructionStarted>(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        base.Update(elapsedTime);

        if (switchToDefaultRenderer)
        {
            Owner.RemoveComponent(this);
            Owner.AddComponent(new DefaultComponentRenderer());
        }
    }

    protected override IDrawableSprite<TVertexData> Drawable<TVertex, TVertexData>(
        SpriteDrawInfo<TVertex, TVertexData> sprite)
    {
        return sprite.Sprite.MakeConcreteWith(
            Owner.Game.Meta.SpriteRenderers,
            SpriteDrawGroup.Particle, 0,
            sprite.Create, shader);
    }

    public void HandleEvent(ConstructionStarted @event)
    {
        switchToDefaultRenderer = true;
    }
}
