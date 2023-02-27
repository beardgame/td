using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage;

[Component("spawnObjectOnTakeHit")]
sealed class SpawnObjectOnTakeHit
    : Component<SpawnObjectOnTakeHit.IParameters>, IListener<TakeHit>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        float? ScaleFromDamage { get; }

        IGameObjectBlueprint Object { get; }
    }

    public SpawnObjectOnTakeHit(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(TakeHit @event)
    {
        var hitInfo = @event.Context.Impact;

        var obj = GameObjectFactory
            .CreateFromBlueprintWithDefaultRenderer(
                Parameters.Object,
                Owner,
                hitInfo is { } h ? h.Point : Owner.Position,
                Direction2.Zero);

        if (hitInfo is { } hit)
            obj.AddComponent(new Property<Impact>(hit));

        if (Parameters.ScaleFromDamage is { } scalar)
        {
            var scale = new Scale(@event.ActualDamage.Amount.NumericValue * scalar);
            obj.AddComponent(new Property<Scale>(scale));
        }

        Owner.Game.Add(obj);
    }
}
