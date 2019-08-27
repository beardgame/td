using amulware.Graphics;
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
                buttons[i] = new Button().WithDefaultStyle(new ActionBarButtonLabel())
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
                buttons[i].FirstChildOfType<ActionBarButtonLabel>().SetLabelText(actionName, cost);
                buttons[i].IsEnabled = actionName != null;
            }
        }

        private class ActionBarButtonLabel : CompositeControl
        {
            private readonly Label nameLabel;
            private readonly Label costLabel;

            public ActionBarButtonLabel()
            {
                nameLabel = new Label { Color = Color.White, FontSize = 16 };
                Add(nameLabel.Anchor(a => a.Bottom(relativePercentage: .6)));

                costLabel = new Label { Color = Constants.Game.GameUI.ResourcesColor, FontSize = 12 };
                Add(costLabel.Anchor(a => a.Top(relativePercentage: .4)));
            }

            public void SetLabelText(string actionName, string cost)
            {
                nameLabel.Text = actionName ?? "";
                costLabel.Text = cost ?? "";
            }
        }
    }
}
