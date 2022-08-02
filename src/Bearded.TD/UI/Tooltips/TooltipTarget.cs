using Bearded.UI.Controls;
using Bearded.UI.EventArgs;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Tooltips;

sealed class TooltipTarget : Control
{
    private readonly TooltipFactory factory;
    private readonly TooltipDefinition definition;
    private readonly TooltipAnchor.Direction direction;
    private Tooltip? tooltip;

    public TooltipTarget(TooltipFactory factory, TooltipDefinition definition, TooltipAnchor.Direction direction)
    {
        this.factory = factory;
        this.definition = definition;
        this.direction = direction;
    }

    protected override void OnAddingToParent()
    {
        base.OnAddingToParent();
        if (Parent is Control control)
        {
            control.MouseEnter += onParentMouseEntered;
            control.MouseExit += onParentMouseExited;
        }
    }

    protected override void OnRemovingFromParent()
    {
        if (Parent is Control control)
        {
            control.MouseEnter -= onParentMouseEntered;
            control.MouseExit -= onParentMouseExited;
        }
        base.OnRemovingFromParent();
    }

    private void onParentMouseEntered(MouseEventArgs t)
    {
        tooltip ??= factory.ShowTooltip(definition, new TooltipAnchor((Control) Parent!, direction));
    }

    private void onParentMouseExited(MouseEventArgs t)
    {
        tooltip?.Destroy();
        tooltip = null;
    }

    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
}
