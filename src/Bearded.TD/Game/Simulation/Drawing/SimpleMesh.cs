using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing;

readonly record struct MeshDrawInfo(IMesh Mesh, Shader Shader, DrawOrderGroup DrawGroup, int DrawGroupOrderKey);

[Component("simpleMesh")]
sealed class SimpleMesh(SimpleMesh.IParameters parameters)
    : Component<SimpleMesh.IParameters>(parameters), IListener<DrawComponents>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        IMesh Mesh { get; }
        Shader? Shader { get; }
        DrawOrderGroup? DrawGroup { get; }
        int DrawGroupOrderKey { get; }
        Difference3 Offset { get; }
        Direction2? Direction { get; }
        [Modifiable(1)]
        Unit Size { get; }
    }

    private MeshDrawInfo mesh;

    protected override void OnAdded() { }

    public override void Activate()
    {
        Events.Subscribe(this);

        mesh = new MeshDrawInfo(
            Parameters.Mesh,
            Parameters.Shader ?? Owner.Game.Meta.Blueprints.Shaders[ModAwareId.ForDefaultMod("deferred-mesh")],
            Parameters.DrawGroup ?? DrawOrderGroup.Building,
            Parameters.DrawGroupOrderKey
            );
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe(this);
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void HandleEvent(DrawComponents e)
    {
        var pos = Owner.Position + Parameters.Offset;
        var dir = Parameters.Direction ?? Owner.Direction;

        e.Drawer.DrawMesh(mesh, pos, dir, Parameters.Size);
    }
}
