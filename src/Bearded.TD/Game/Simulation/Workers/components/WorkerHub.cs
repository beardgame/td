using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.Geometry;
using static Bearded.TD.Utilities.DebugAssert;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Workers;

[Component("workerHub")]
sealed class WorkerHub : Component<WorkerHub.IParameters>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(Type = AttributeType.DroneCount)] int NumWorkers { get; }

        IComponentOwnerBlueprint Drone { get; }
    }

    private IBuildingState? state;
    private IFactionProvider? factionProvider;
    private Faction? faction;
    private int numWorkersActive;

    public WorkerHub(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IBuildingStateProvider>(Owner, Events, p => state = p.State);
        ComponentDependencies.Depend<IFactionProvider>(Owner, Events, provider =>
        {
            factionProvider = provider;
            faction = provider.Faction;
        });
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (faction == null || !(state?.IsFunctional ?? false))
        {
            return;
        }

        if (factionProvider?.Faction != faction)
        {
            State.IsInvalid("Worker hubs cannot change from one faction to another.");
        }

        while (numWorkersActive < Parameters.NumWorkers)
        {
            addNewWorker();
        }
    }

    private void addNewWorker()
    {
        State.Satisfies(faction != null);
        var obj = GameObjectFactory.CreateFromBlueprintWithDefaultRenderer(
            Parameters.Drone, Owner, Owner.Position, Direction2.Zero);
        obj.AddComponent(new FactionProvider(faction!));
        Owner.Game.Add(obj);

        numWorkersActive++;
    }
}
