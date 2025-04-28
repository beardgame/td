using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameObjects;

using static SpawnObjectOnTrigger;

[Component("spawnObjectOnTrigger")]
sealed class SpawnObjectOnTrigger(IParameters parameters) : Component<IParameters>(parameters)
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        ITrigger Trigger { get; }
        IGameObjectBlueprint Object { get; }
        Unit OffsetInDirection { get; }
    }

    public override void Activate()
    {
        Parameters.Trigger.Subscribe(Events, spawn);
    }

    private void spawn()
    {
        var d = Owner.Direction;
        var p = Owner.Position + (d * Parameters.OffsetInDirection).WithZ();

        var obj = GameObjectFactory.CreateFromBlueprintWithDefaultRenderer(Parameters.Object, Owner, p, d);
        Owner.Game.Add(obj);
    }
}
