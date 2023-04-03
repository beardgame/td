using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings;

interface IEmergencyEMPReport : IReport
{
    bool Available { get; }
    GameObject Owner { get; }
}

[Component("emergencyEMP")]
sealed class EmergencyEMP : Component<EmergencyEMP.IParameters>, IEmergencyEMPReport, IListener<WaveStarted>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        IGameObjectBlueprint Object { get; }
    }

    public bool Available { get; private set; } = true;

    public EmergencyEMP(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        ReportAggregator.Register(Events, this);
    }

    public override void Activate()
    {
        Owner.Game.Meta.Events.Subscribe(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(WaveStarted e)
    {
        Available = true;
    }

    public void Fire()
    {
        var obj = GameObjectFactory.CreateFromBlueprintWithDefaultRenderer(Parameters.Object, Owner, Owner.Position);
        Owner.Game.Add(obj);

        Available = false;
    }

    ReportType IReport.Type => ReportType.ManualControl;
    GameObject IEmergencyEMPReport.Owner => base.Owner;
}
