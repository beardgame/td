using System;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing;

[Component("simpleMesh")]
sealed class SimpleMesh : Component<SimpleMesh.IParameters>, IListener<DrawComponents>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        IMesh Mesh { get; }
        Shader? Shader { get; }
        DrawOrderGroup? DrawGroup { get; }
        int DrawGroupOrderKey { get; }
        Difference3 Offset { get; }
        Direction2? Direction { get; }
    }

    private Shader? shader;
    private DrawOrderGroup drawGroup;

    public SimpleMesh(IParameters parameters) : base(parameters) { }

    protected override void OnAdded() { }

    public override void Activate()
    {
        Events.Subscribe(this);
        shader = Parameters.Shader ?? Owner.Game.Meta.Blueprints.Shaders[ModAwareId.ForDefaultMod("deferred-mesh")];
        drawGroup = Parameters.DrawGroup ?? DrawOrderGroup.Building;
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe(this);
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void HandleEvent(DrawComponents e)
    {
        if (shader is null)
        {
            throw new Exception("Received draw event before activation.");
        }

        // TODO: use IComponentDrawer interface for this
        var drawableMesh = Parameters.Mesh.AsDrawable(
            Owner.Game.Meta.DrawableRenderers, drawGroup,Parameters.DrawGroupOrderKey, shader);

        var pos = Owner.Position + Parameters.Offset;
        var dir = Parameters.Direction ?? Owner.Direction;
        drawableMesh.Add(pos.NumericValue, dir - Direction2.Zero);
    }
}
