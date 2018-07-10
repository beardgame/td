using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using OpenTK;

namespace Bearded.TD.UI.Controls
{
    sealed class GameStatusUIControl : CompositeControl
    {
        private readonly GameStatusUI model;
        private readonly Label resourcesLabel;

        public GameStatusUIControl(GameStatusUI model)
        {
            this.model = model;

            Add(new BackgroundBox());

            Add(new Label("Resources:")
                {
                    FontSize = 16,
                    TextAnchor = new Vector2d(0, .5)
                }
                .Anchor(a => a
                    .Top(margin: 8, height: 24)
                    .Left(margin: 8)));

            resourcesLabel = new Label
                {
                    FontSize = 16,
                    TextAnchor = new Vector2d(1, .5)
                }
                .Anchor(a => a
                    .Top(margin: 8, height: 24)
                    .Right(margin: 8));
            Add(resourcesLabel);

            model.StatusChanged += updateLabels;
        }

        private void updateLabels()
        {
            resourcesLabel.Text = $"{model.FactionResources} {model.FactionResourceIncome:+#;-#;0}";
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }
}
