using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Components.Generic;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.UI.Layers;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls
{
    sealed class BuildingStatusUIControl : CompositeControl
    {
        private readonly ListControl upgradeList = new ListControl(new ViewportClippingLayerControl());
        
        private BuildingStatusUI buildingStatus;
        private Building building;
        private UpgradeListItemSource upgradeItemSource;

        public BuildingStatusUIControl(BuildingStatusUI buildingStatus)
        {
            this.buildingStatus = buildingStatus;
            building = buildingStatus.Building as Building;
            
            Add(new BackgroundBox());

            Add(new Label(buildingStatus.Building.Blueprint.Name) {FontSize = 24}
                .Anchor(a => a.Top(margin: 4, height: 24).Left(margin: 4).Right(margin: 4)));
            Add(new Label($"Owned by {buildingStatus.Building.Faction.Name ?? "nobody"}") {FontSize = 16}
                .Anchor(a => a.Top(margin: 32, height: 16).Left(margin: 4).Right(margin: 4)));

            if (building?.GetComponent<Health>() is Health health)
            {
                Add(new DynamicLabel(() => $"Hitpoints: {health.CurrentHealth} / {health.MaxHealth}")
                        {FontSize = 16}
                    .Anchor(a => a.Top(margin: 52, height: 16).Left(margin: 4).Right(margin: 4)));
            }

            Add(upgradeList.Anchor(a => a.Top(margin: 72).Bottom(margin: 40).Left(margin: 4).Right(margin: 4)));

            Add(Default.Button("Close")
                .Anchor(a => a.Bottom(margin: 4, height: 32).Left(margin: 4).Right(relativePercentage: .5, margin: 2))
                .Subscribe(btn => btn.Clicked += buildingStatus.OnCloseClicked));
            Add(Default.Button("Delete")
                .Anchor(a => a.Bottom(margin: 4, height: 32).Right(margin: 4).Left(relativePercentage: .5, margin: 2))
                .Subscribe(btn => btn.Clicked += buildingStatus.OnDeleteBuildingClicked));
            
            updateUpgradeList();
        }
        
        public override void Render(IRendererRouter r)
        {
            updateUpgradeListIfNeeded();
            
            base.Render(r);
        }

        private void updateUpgradeListIfNeeded()
        {
            if (upgradeItemSource?.AppliedUpgradesCount != building?.AppliedUpgrades.Count)
                updateUpgradeList();
        }

        private void updateUpgradeList()
        {
            upgradeItemSource = new UpgradeListItemSource(
                buildingStatus.Game, building, buildingStatus.UpgradesForBuilding);
            upgradeList.ItemSource = upgradeItemSource;
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);

        private class UpgradeListItemSource : IListItemSource
        {
            private readonly GameInstance game;
            private readonly Building building;
            private readonly List<UpgradeBlueprint> availableUpgrades;
            private readonly List<UpgradeBlueprint> appliedUpgrades;

            public int ItemCount { get; }

            public int AppliedUpgradesCount => appliedUpgrades.Count;

            public UpgradeListItemSource(GameInstance game, Building building, IEnumerable<UpgradeBlueprint> upgrades)
            {
                this.game = game;
                this.building = building;
                appliedUpgrades = building?.AppliedUpgrades.ToList() ?? new List<UpgradeBlueprint>();
                availableUpgrades = upgrades.ToList();
                ItemCount = availableUpgrades.Count + appliedUpgrades.Count;
            }

            public double HeightOfItemAt(int index)
            {
                return indexIsAvailableUpgrade(index) ? 32 : 16;
            }

            public Control CreateItemControlFor(int index)
            {
                return indexIsAvailableUpgrade(index)
                    ? createAvailableUpgradeButton(index)
                    : createAppliedUpgradeControl(index);
            }

            private Control createAppliedUpgradeControl(int index)
            {
                var upgrade = appliedUpgrades[index - availableUpgrades.Count];
                var control = new AppliedUpgradeControl(upgrade);
                return control;
            }

            private Control createAvailableUpgradeButton(int index)
            {
                var upgrade = availableUpgrades[index];
                var ctrl = new UpgradeButton(building, upgrade);
                ctrl.Clicked += () => game.Request(UpgradeBuilding.Request, building, upgrade);
                return ctrl;
            }

            private bool indexIsAvailableUpgrade(int index)
            {
                return index < availableUpgrades.Count;
            }

            public void DestroyItemControlAt(int index, Control control) {}
        }

        private class AppliedUpgradeControl : Label
        {
            public AppliedUpgradeControl(UpgradeBlueprint upgrade)
            {
                Text = upgrade.Name;
                FontSize = 16;
            }

            protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
        }
        
        private class UpgradeButton : Button
        {
            private readonly Building building;
            private readonly UpgradeBlueprint upgrade;
            private readonly BackgroundBox progressBar;

            public UpgradeButton(Building building, UpgradeBlueprint upgrade)
            {
                this.building = building;
                this.upgrade = upgrade;
                this.WithDefaultStyle(upgrade.Name);
                progressBar = new BackgroundBox { Color = Color.White * 0.25f };
                Add(progressBar);
            }

            public override void Render(IRendererRouter r)
            {
                var activeUpgrade = building.UpgradesInProgress.FirstOrDefault(task => task.Upgrade == upgrade);
                var upgradeIsActive = activeUpgrade != null;

                IsEnabled = !upgradeIsActive;
                progressBar.IsVisible = upgradeIsActive;
                
                if (upgradeIsActive)
                {
                    var percentage = activeUpgrade.ProgressPercentage;
                    progressBar.Anchor(a => a.Right(relativePercentage: percentage));
                }

                base.Render(r);
            }
        }
    }
}
