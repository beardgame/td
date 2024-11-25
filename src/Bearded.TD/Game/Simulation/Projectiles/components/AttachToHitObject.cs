using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Simulation.Projectiles.AttachToHitObject;

namespace Bearded.TD.Game.Simulation.Projectiles;

[Component("attachToHitObject")]
sealed class AttachToHitObject(IParameters parameters)
    : Component<IParameters>(parameters)
{
    private GameObject? attachedObject;

    internal interface IParameters : IParametersTemplate<IParameters>
    {
    }

    protected override void OnAdded()
    {
    }

    public override void Activate()
    {
        if (Owner.TryGetProperty<HitObject>(out var hitObject))
        {
            attachedObject = hitObject.Object;
        }
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (attachedObject is { } obj)
        {
            Owner.Position = obj.Position;
        }
    }
}
