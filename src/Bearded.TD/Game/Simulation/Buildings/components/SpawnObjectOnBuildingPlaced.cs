using System;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.Geometry;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings;

[Component("spawnObjectOnBuildingPlaced")]
sealed class SpawnObjectOnBuildingPlaced
    : Component<SpawnObjectOnBuildingPlaced.IParameters>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        IGameObjectBlueprint Object { get; }
    }

    public SpawnObjectOnBuildingPlaced(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
    }

    public override void Activate()
    {
        var building = Owner.GetComponents<IBuildingStateProvider>().First();

        if (!building.State.IsGhost)
            spawnObject();

        Owner.RemoveComponent(this);
    }

    private void spawnObject()
    {
        var obj = GameObjectFactory
            .CreateFromBlueprintWithDefaultRenderer(Parameters.Object, Owner, Owner.Position, Direction2.Zero);
        Owner.Game.Add(obj);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        throw new InvalidOperationException();
    }
}
