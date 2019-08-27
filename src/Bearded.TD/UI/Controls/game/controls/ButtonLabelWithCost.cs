using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls
{
    sealed class ButtonLabelWithCost : CompositeControl
    {
        private readonly Label nameLabel;
        private readonly Label costLabel;

        public string Name
        {
            get => nameLabel.Text;
            set => nameLabel.Text = value ?? "";
        }

        public string Cost
        {
            get => costLabel.Text;
            set => costLabel.Text = value ?? "";
        }

        public ButtonLabelWithCost()
        {
            nameLabel = new Label { FontSize = 16 };
            Add(nameLabel.Anchor(a => a.Bottom(relativePercentage: .6)));

            costLabel = new Label { Color = Constants.Game.GameUI.ResourcesColor, FontSize = 12 };
            Add(costLabel.Anchor(a => a.Top(relativePercentage: .4)));
        }
    }
}
