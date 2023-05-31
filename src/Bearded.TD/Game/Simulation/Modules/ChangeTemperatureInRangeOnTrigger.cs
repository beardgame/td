using Bearded.TD.Game.Simulation.Elements;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Modules;

[Component("changeTemperatureInRangeOnTrigger")]
sealed class ChangeTemperatureInRangeOnTrigger : Component<ChangeTemperatureInRangeOnTrigger.IParameters>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        ITrigger Trigger { get; }

        [Modifiable(Type = AttributeType.SplashRange)]
        Unit Range { get; }

        [Modifiable(10)]
        TemperatureDifference TemperatureDifference { get; }
    }

    private ITriggerSubscription? subscription;

    public ChangeTemperatureInRangeOnTrigger(IParameters parameters) : base(parameters) { }

    protected override void OnAdded() { }

    public override void Activate()
    {
        subscription = Parameters.Trigger.Subscribe(Events, applyTemperatureChange);
    }

    private void applyTemperatureChange()
    {
        var objectsInRange = AreaOfEffect.FindObjectsInLayer(
            Owner.Game.TemperatureLayer,
            Owner.Position,
            Parameters.Range);
        foreach (var obj in objectsInRange)
        {
            if (obj.GameObject.TryGetSingleComponent<ITemperatureEventReceiver>(out var temperature))
            {
                temperature.ApplyImmediateTemperatureChange(Parameters.TemperatureDifference);
            }
            else
            {
                DebugAssert.State.IsInvalid("Found object in temperature layer without temperature event receiver.");
            }
        }
    }

    public override void OnRemoved()
    {
        subscription?.Unsubscribe(Events);
        base.OnRemoved();
    }

    public override void Update(TimeSpan elapsedTime) { }
}
