using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Components.Generic;
using Bearded.TD.Game.Upgrades;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls
{
    sealed class BuildingStatusUIControl : CompositeControl
    {
        public BuildingStatusUIControl(BuildingStatusUI buildingStatus)
        {
            var building = buildingStatus.Building as Building;
            var health = building?.GetComponent<Health>();

            Add(new BackgroundBox());

            Add(new Label(buildingStatus.Building.Blueprint.Name) {FontSize = 24}
                .Anchor(a => a.Top(margin: 4, height: 24).Left(margin: 4).Right(margin: 4)));
            Add(new Label($"Owned by {buildingStatus.Building.Faction.Name ?? "nobody"}") {FontSize = 16}
                .Anchor(a => a.Top(margin: 32, height: 16).Left(margin: 4).Right(margin: 4)));

            if (health != null)
            {
                Add(new DynamicLabel(() => $"Hitpoints: {health.CurrentHealth} / {health.MaxHealth}")
                        {FontSize = 16}
                    .Anchor(a => a.Top(margin: 52, height: 16).Left(margin: 4).Right(margin: 4)));
            }

            Add(new ListControl
                    {ItemSource = new UpgradeListItemSource(building, buildingStatus.UpgradesForBuilding)}
                .Anchor(a => a.Top(margin: 72).Bottom(margin: 40).Left(margin: 4).Right(margin: 4)));

            Add(Default.Button("Close")
                .Anchor(a => a.Bottom(margin: 4, height: 32).Left(margin: 4).Right(relativePercentage: .5, margin: 2))
                .Subscribe(btn => btn.Clicked += buildingStatus.OnCloseClicked));
            Add(Default.Button("Delete")
                .Anchor(a => a.Bottom(margin: 4, height: 32).Right(margin: 4).Left(relativePercentage: .5, margin: 2))
                .Subscribe(btn => btn.Clicked += buildingStatus.OnDeleteBuildingClicked));
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);

        private class UpgradeListItemSource : IListItemSource
        {
            private readonly Building building;
            private readonly List<UpgradeBlueprint> upgrades;
            
            public int ItemCount { get; }

            public UpgradeListItemSource(Building building, IEnumerable<UpgradeBlueprint> upgrades)
            {
                this.building = building;
                this.upgrades = upgrades.ToList();
                ItemCount = this.upgrades.Count;
            }

            public double HeightOfItemAt(int index)
            {
                return 32;
            }

            public Control CreateItemControlFor(int index)
            {
                var upgrade = upgrades[index];
                var ctrl = new Button
                {
                    new Label(upgrade.Name) {FontSize = 14}
                };
                ctrl.Clicked += () => building.ApplyUpgrade(upgrade);
                return ctrl;
            }

            public void DestroyItemControlAt(int index, Control control) {}
        }
    }
}
