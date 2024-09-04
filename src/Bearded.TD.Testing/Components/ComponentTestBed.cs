using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Testing.GameStates;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Testing.Components;

sealed class ComponentTestBed
{
    private readonly GameTestBed gameTestBed;
    private readonly ComponentInternals internals = new();
    private readonly GameObject obj;

    private ComponentTestBed(GameTestBed? gameTestBed = null)
    {
        this.gameTestBed = gameTestBed ?? GameTestBed.Create();
        obj = GameObjectFactory.CreateWithoutRenderer(null, Position3.Zero);
        obj.AddComponent(internals);
    }

    public static ComponentTestBed CreateInGame(GameTestBed? gameTestBed = null)
    {
        var testBed = CreateOrphaned(gameTestBed);
        testBed.AddToGameState();
        return testBed;
    }

    public static ComponentTestBed CreateOrphaned(GameTestBed? gameTestBed = null)
    {
        return new ComponentTestBed(gameTestBed);
    }

    public void AddToGameState()
    {
        gameTestBed.State.Add(obj);
    }

    public void AddComponent(IComponent component) => obj.AddComponent(component);

    public void RemoveComponent(IComponent component) => obj.RemoveComponent(component);

    public IEnumerable<TComponent> GetComponents<TComponent>() => obj.GetComponents<TComponent>();

    public void SendEvent<T>(T @event) where T : struct, IComponentEvent
    {
        internals.SendEvent(@event);
    }

    public void PreviewEvent<T>(ref T @event) where T : struct, IComponentPreviewEvent
    {
        internals.PreviewEvent(ref @event);
    }

    public Queue<T> CollectEvents<T>() where T : struct, IComponentEvent
    {
        var q = new Queue<T>();
        internals.Subscribe(new LambdaListener<T>(q.Enqueue));
        return q;
    }

    public void AdvanceFramesFor(TimeSpan duration)
    {
        gameTestBed.AdvanceFramesFor(duration);
    }

    public void AdvanceSingleFrame()
    {
        gameTestBed.AdvanceSingleFrame();
    }

    public void MoveObject(Position3 pos)
    {
        obj.Position = pos;
    }

    private sealed class ComponentInternals : Component
    {
        protected override void OnAdded() {}
        public override void Update(TimeSpan elapsedTime) {}

        public void SendEvent<T>(T @event) where T : struct, IComponentEvent
        {
            Events.Send(@event);
        }

        public void PreviewEvent<T>(ref T @event) where T : struct, IComponentPreviewEvent
        {
            Events.Preview(ref @event);
        }

        public void Subscribe<T>(IListener<T> listener) where T : struct, IComponentEvent
        {
            Events.Subscribe(listener);
        }
    }

    private sealed class LambdaListener<T>(Action<T> onEvent) : IListener<T>
        where T : IEvent
    {
        public void HandleEvent(T @event) => onEvent(@event);
    }
}
