using Bearded.UI.Controls;

namespace Bearded.TD.UI.Model
{
    abstract class UIModel
    {
        private readonly Control viewControl;

        protected UIModel(Control viewControl)
        {
            this.viewControl = viewControl;
        }

        protected void Destroy()
        {
            viewControl.RemoveFromParent();
        }
    }
}
