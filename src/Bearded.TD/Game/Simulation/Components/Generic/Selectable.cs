using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.Selection.events;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Components.Generic
{
    [Component("selectable")]
    sealed class Selectable<T> : Component<T>, ISelectable
    {
        public SelectionState SelectionState { get; private set; }

        protected override void Initialize()
        {
            throw new System.NotImplementedException();
        }

        public void ResetSelection()
        {
            var oldState = SelectionState;
            SelectionState = SelectionState.Default;

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (oldState)
            {
                case SelectionState.Focused:
                    Events.Send(new ObjectFocusReset());
                    break;
                case SelectionState.Selected:
                    Events.Send(new ObjectSelectionReset());
                    break;
            }
        }

        public void Focus()
        {
            SelectionState = SelectionState.Focused;
            Events.Send(new ObjectFocused());
        }

        public void Select()
        {
            SelectionState = SelectionState.Selected;
            Events.Send(new ObjectSelected());
        }

        public override void Update(TimeSpan elapsedTime) {}

        public override void Draw(CoreDrawers drawers) {}
    }
}
