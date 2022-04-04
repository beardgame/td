using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Testing.GameStates;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Testing.Components;

sealed class ComponentTestBed
{
    private readonly GameTestBed gameTestBed;
    private readonly ComponentInternals internals = new();
    private readonly GameObject obj = new(null, Position3.Zero, Direction2.Zero);

    public ComponentTestBed(GameTestBed? gameTestBed = null)
    {
        this.gameTestBed = gameTestBed ?? GameTestBed.Create();
        obj.AddComponent(internals);
        this.gameTestBed.State.Add(obj);
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

    public void AdvanceSingleFrame()
    {
        gameTestBed.AdvanceSingleFrame();
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
    }
}
