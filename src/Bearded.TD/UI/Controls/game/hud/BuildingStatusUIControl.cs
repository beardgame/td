﻿using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls
{
    sealed class BuildingStatusUIControl : CompositeControl
    {
        public BuildingStatusUIControl(BuildingStatusUI buildingStatus)
        {
            Add(new Label(buildingStatus.Building.Blueprint.Name) { FontSize = 24 }
                .Anchor(a => a.Top(margin: 4, height: 24).Left(margin: 4).Right(margin: 4)));
            Add(new Label($"Owned by {buildingStatus.Building.Faction.Color.ToString()}") { FontSize = 16 }
                .Anchor(a => a.Top(margin: 32, height: 16).Left(margin: 4).Right(margin: 4)));
            Add(new DynamicLabel(() => $"Hitpoints: {0} / {buildingStatus.Building.Blueprint.MaxHealth}") { FontSize = 16 }
                .Anchor(a => a.Top(margin: 52, height: 16).Left(margin: 4).Right(margin: 4)));

            // Listview
            Add(new SimpleControl()
                .Anchor(a => a.Top(margin: 72).Bottom(margin: 40).Left(margin: 4).Right(margin: 4)));

            Add(new Button { new Label("Close") }
                .Anchor(a => a.Bottom(margin: 4, height: 32).Left(margin: 4).Right(relativePercentage: .5, margin: 2))
                .Subscribe(btn => buildingStatus.OnCloseClicked()));
            Add(new Button { new Label("Delete") }
                .Anchor(a => a.Bottom(margin: 4, height: 32).Right(margin: 4).Left(relativePercentage: .5, margin: 2))
                .Subscribe(btn => buildingStatus.OnDeleteBuildingClicked()));
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }
}
