using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Selection;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

class InputAwareStatusDisplayCondition : IStatusDisplayCondition
{
    private IUIDrawState? uiDrawState;
    private SelectionListener? selectionListener;
    private bool isHovered;

    public virtual bool ShouldDraw => isHovered || (uiDrawState?.DrawStatusDisplays ?? true);

    public virtual void Activate(GameObject owner, ComponentEvents events)
    {
        uiDrawState = owner.Game.Meta.UIDrawState;
        selectionListener = SelectionListener.Create(
            onFocus: () => isHovered = true,
            onFocusReset: () => isHovered = false);
        selectionListener.Subscribe(events);
    }
}
