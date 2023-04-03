using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using JetBrains.Annotations;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Modules;

[Component("spawnObjectOnSprintStart")]
sealed class SpawnObjectOnSprintStart
    : Component<SpawnObjectOnSprintStart.IParameters>, IListener<StartedSprinting>, IListener<StoppedSprinting>
{
    private GameObject? obj;

    public interface IParameters : IParametersTemplate<IParameters>
    {
        IGameObjectBlueprint Object { get; }
        AttachBehaviour Attach { get; }
        DeletionBehaviour Delete { get; }
        TimeSpan? DeletionDelay { get; }
    }

    public enum AttachBehaviour
    {
        Never = 0,
        [UsedImplicitly]
        Always,
        WhileSprinting,
    }
    public enum DeletionBehaviour
    {
        [UsedImplicitly]
        Never = 0,
        AfterSprintStart,
        AfterSprintStop,
    }

    public SpawnObjectOnSprintStart(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        Events.Subscribe<StartedSprinting>(this);
        Events.Subscribe<StoppedSprinting>(this);
    }

    public override void Activate()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(StartedSprinting e)
    {
        obj = GameObjectFactory.CreateFromBlueprintWithDefaultRenderer(Parameters.Object, Owner, Owner.Position);

        if (Parameters.Delete == DeletionBehaviour.AfterSprintStart)
        {
            deleteObjectAfter(Parameters.DeletionDelay ?? 0.S());
        }

        if (Parameters.Attach != AttachBehaviour.Never)
        {
            obj.AddComponent(new AttachToParent());
        }

        Owner.Game.Add(obj);
    }

    public void HandleEvent(StoppedSprinting e)
    {
        if (obj == null)
            return;

        if (Parameters.Delete == DeletionBehaviour.AfterSprintStop)
        {
            switch (Parameters.DeletionDelay)
            {
                case { } d when d < 0.S():
                    obj.Delete();
                    break;
                case { } d:
                    deleteObjectAfter(d);
                    break;
            }
        }

        if (!obj.Deleted && Parameters.Attach == AttachBehaviour.WhileSprinting)
        {
            obj.RemoveComponent(obj.GetComponents<AttachToParent>().First());
        }

        obj = null;
    }

    private void deleteObjectAfter(TimeSpan d)
    {
        obj?.AddComponent(new DeleteAfter(new DeleteAfterParametersTemplate(d)));
    }
}

