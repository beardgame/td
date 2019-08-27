using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls
{
    sealed class ActionBarControl : CompositeControl
    {
        private const float buttonHeightPercentage = 1f / Constants.Game.GameUI.ActionBarSize;

        private readonly ActionBar model;
        private readonly Button[] buttons;

        public ActionBarControl(ActionBar model)
        {
            this.model = model;

            Add(new BackgroundBox());

            buttons = new Button[Constants.Game.GameUI.ActionBarSize];
            for (var i = 0; i < buttons.Length; i++)
            {
                var i1 = i;
                buttons[i] = new Button().WithDefaultStyle(new ButtonLabelWithCost())
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
                var (actionName, cost) = model.ActionLabelForIndex(i);
                var labelWithCost = buttons[i].FirstChildOfType<ButtonLabelWithCost>();
                labelWithCost.Name = actionName;
                labelWithCost.Cost = cost;
                buttons[i].IsEnabled = actionName != null;
            }
        }
    }
}
