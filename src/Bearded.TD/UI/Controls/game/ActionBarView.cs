using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls
{
    sealed class ActionBarView : CompositeControl
    {
        private const float buttonHeightPercentage = 1f / ActionBar.NumActions;
        
        private readonly ActionBar model;
        private readonly LabeledButton<string>[] buttons;

        public ActionBarView(ActionBar model)
        {
            this.model = model;
            buttons = new LabeledButton<string>[ActionBar.NumActions];
            for (var i = 0; i < ActionBar.NumActions; i++)
            {
                var i1 = i;
                buttons[i] = new LabeledButton<string>("")
                    .Anchor(a => a.Top(i1 * buttonHeightPercentage).Bottom((i1 + 1) * buttonHeightPercentage))
                    .Subscribe(b => model.OnActionClicked(i1));
                Add(buttons[i]);
            }
            updateButtonLabels();
        }
        
        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);

        private void updateButtonLabels()
        {
            for (var i = 0; i < buttons.Length; i++)
            {
                var label = model.ActionLabelForIndex(i);
                buttons[i].Label = label ?? "";
                buttons[i].Enabled = label != null;
            }
        }
    }
}
