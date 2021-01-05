using amulware.Graphics;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls
{
    sealed class BuildingStatusOverlayControl : CompositeControl
    {
        private readonly BuildingStatusOverlay model;

        private readonly Binding<string> name = new();
        private readonly Binding<string> ownerName = new();
        private readonly Binding<Color> ownerColor = new();
        private readonly Binding<string> health = new();

        public BuildingStatusOverlayControl(BuildingStatusOverlay model)
        {
            IsClickThrough = true;

            this.model = model;
            this.BuildLayout()
                .ForFullScreen()
                .AddEntityStatus(b => b
                    .WithName(name)
                    .AddTextAttribute("Owned by", ownerName, ownerColor)
                    .AddTextAttribute("Health", health)
                    .WithCloseAction(model.Close));

            updateBuildingAttributes();
            onBuildingUpdated();

            model.BuildingUpdated += onBuildingUpdated;
        }

        private void onBuildingUpdated()
        {
            updateHealth();
        }

        private void updateBuildingAttributes()
        {
            name.SetFromSource(model.Building.Blueprint.Name);
            ownerName.SetFromSource(model.Building.Faction.Name);
            ownerColor.SetFromSource(model.Building.Faction.Color);
        }

        private void updateHealth()
        {
            var h = model.BuildingHealth;
            health.SetFromSource(h == null ? "N/A" : $"{h.Value.CurrentHealth} / {h.Value.MaxHealth}");
        }
    }
}
