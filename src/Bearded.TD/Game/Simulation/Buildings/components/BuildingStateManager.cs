using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Selection;
using Bearded.TD.Game.Simulation.Synchronization;
using Bearded.TD.Rendering;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings
{
    sealed class BuildingStateManager<T>
        : Component<T>,
            IBuildingStateProvider,
            IListener<ConstructionFinished>,
            IListener<ConstructionStarted>
        where T : IComponentOwner<T>, IDeletable, IIdable<T>, IGameObject
    {
        private readonly BuildingState state = new();

        public IBuildingState State { get; }

        public BuildingStateManager()
        {
            State = state.CreateProxy();
        }

        protected override void Initialize() {
            SelectionListener.Create(
                    onFocus: () => state.SelectionState = SelectionState.Focused,
                    onFocusReset: () => state.SelectionState = SelectionState.Default,
                    onSelect: () => state.SelectionState = SelectionState.Selected,
                    onSelectionReset: () => state.SelectionState = SelectionState.Default)
                .Subscribe(Events);
            Events.Subscribe<ConstructionFinished>(this);
            Events.Subscribe<ConstructionStarted>(this);
        }

        public void HandleEvent(ConstructionFinished @event)
        {
            state.IsCompleted = true;
        }

        public void HandleEvent(ConstructionStarted @event)
        {
            materialize();
        }

        private void materialize()
        {
            Owner.AddComponent(new Syncer<T>());
            state.IsMaterialized = true;
            Events.Send(new Materialized());
        }

        public override void Update(TimeSpan elapsedTime) { }
        public override void Draw(CoreDrawers drawers) { }
    }
}
