﻿using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    sealed class GameStatusUIControl : CompositeControl
    {
        private readonly GameStatusUI model;
        private readonly Binding<string> resourcesAmount = new();
        private readonly Binding<string> techPointsAmount = new();

        public event VoidEventHandler? TechnologyButtonClicked;

        public GameStatusUIControl(GameStatusUI model)
        {
            this.model = model;

            Add(new BackgroundBox());

            var content = new CompositeControl().Anchor(a => a.MarginAllSides(4));
            content.BuildFixedColumn()
                .AddHeader($"{model.FactionName}", model.FactionColor)
                .AddValueLabel("Resources:", resourcesAmount, rightColor: Constants.Game.GameUI.ResourcesColor)
                .AddValueLabel("Tech points:", techPointsAmount, rightColor: Constants.Game.GameUI.TechPointsColor)
                .AddButton(b => b.WithLabel("Research").WithOnClick(() => TechnologyButtonClicked?.Invoke()));
            this.BuildLayout().ForContentBox().FillContent(content);

            model.StatusChanged += updateLabels;
        }

        private void updateLabels()
        {
            resourcesAmount.SetFromSource($"{model.FactionResources.DisplayValue}");
            techPointsAmount.SetFromSource($"{model.FactionTechPoints}");
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }
}
