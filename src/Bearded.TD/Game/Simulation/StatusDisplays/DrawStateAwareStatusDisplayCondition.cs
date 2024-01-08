using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

class InputAwareStatusDisplayCondition : IStatusDisplayCondition
{
    private IUIDrawState? uiDrawState;

    public virtual bool ShouldDraw => uiDrawState?.DrawStatusDisplays ?? true;

    public virtual void Activate(GameObject owner)
    {
        uiDrawState = owner.Game.Meta.UIDrawState;
    }
}
