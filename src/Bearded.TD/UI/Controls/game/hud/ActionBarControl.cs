using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls
{
    sealed class ActionBarControl : CompositeControl
    {
        private const float buttonHeightPercentage = 1f / Constants.Game.UI.ActionBarSize;
        
        private readonly ActionBar model;
        private readonly Button[] buttons;

        public ActionBarControl(ActionBar model)
        {
            this.model = model;
            buttons = new Button[Constants.Game.UI.ActionBarSize];
            for (var i = 0; i < buttons.Length; i++)
            {
                var i1 = i;
                buttons[i] = new Button { new Label("") { FontSize = 16 } }
                    .Anchor(a => a
                        .Top(relativePercentage: i1 * buttonHeightPercentage)
                        .Bottom(relativePercentage: (i1 + 1) * buttonHeightPercentage))
                    .Subscribe(b => b.Clicked += () => model.OnActionClicked(i1));
                Add(buttons[i]);
            }

            model.ActionsChanged += updateButtonLabels;
            updateButtonLabels();
        }
        
        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);

        private void updateButtonLabels()
        {
            for (var i = 0; i < buttons.Length; i++)
            {
                var label = model.ActionLabelForIndex(i);
                buttons[i].FirstChildOfType<Label>().Text = label ?? "";
                buttons[i].IsEnabled = label != null;
            }
        }
    }
}
