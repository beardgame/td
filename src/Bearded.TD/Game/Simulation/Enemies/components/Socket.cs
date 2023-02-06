using System.Diagnostics.CodeAnalysis;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Enemies;

[Component("socket")]
sealed class Socket : Component<Socket.IParameters>, ISocket
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        SocketShape Shape { get; }
    }

    private AppliedModule? appliedModule;
    public SocketShape Shape => Parameters.Shape;

    public Socket(IParameters parameters) : base(parameters) { }

    protected override void OnAdded() { }

    public override void Activate() { }

    public void Fill(IModule module)
    {
        Argument.Satisfies(module.SocketShape == Shape);
        State.Satisfies(appliedModule == null);

        var receipt = Owner.ApplyUpgrade(module);
        appliedModule = new AppliedModule(module, receipt);
    }

    public override void Update(TimeSpan elapsedTime) { }

    [SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Local")] // TODO: consider if useful
    private sealed record AppliedModule(IModule Module, IUpgradeReceipt Receipt);
}
