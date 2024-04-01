using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.StatusDisplays;

namespace Bearded.TD.Game.Simulation.Elements;

sealed partial class TemperatureProperty
{
    private sealed class TemperatureStatus
    {
        private static readonly ModAwareSpriteId coldStatusIcon = "thermometer-cold".ToStatusIconSpriteId();
        private static readonly ModAwareSpriteId hotStatusIcon = "thermometer-hot".ToStatusIconSpriteId();
        private static readonly ModAwareSpriteId overheatedStatusIcon = "hot-surface".ToStatusIconSpriteId();

        private readonly IStatusTracker statusTracker;
        private readonly IStatusDrawer coldDrawer;
        private readonly IStatusDrawer hotDrawer;
        private readonly IStatusDrawer overheatedDrawer;
        private IStatusReceipt? coldStatus;
        private IStatusReceipt? hotStatus;
        private IStatusReceipt? overheatedStatus;
        private float progress;

        public TemperatureStatus(GameState game, IStatusTracker statusTracker)
        {
            this.statusTracker = statusTracker;
            coldDrawer = new ProgressStatusDrawer(iconStatusDrawer(game, "thermometer-cold"), () => progress);
            hotDrawer = new ProgressStatusDrawer(iconStatusDrawer(game, "thermometer-hot"), () => progress);
            overheatedDrawer = iconStatusDrawer(game, "hot-surface");
        }

        private static IStatusDrawer iconStatusDrawer(GameState game, string iconName) =>
            IconStatusDrawer.FromSpriteBlueprint(game, game.Meta.Blueprints.LoadStatusIconSprite(iconName));

        public void UpdateCurrentTemperature(Temperature newTemperature)
        {
            if (newTemperature < Constants.Game.Elements.MinNormalTemperature)
            {
                progress = (newTemperature - Constants.Game.Elements.MinNormalTemperature) /
                    (Constants.Game.Elements.MinTemperature - Constants.Game.Elements.MinNormalTemperature);

                if (coldStatus is null)
                {
                    coldStatus = statusTracker.AddStatus(
                        new StatusSpec(StatusType.Neutral, null, coldDrawer),
                        StatusAppearance.IconAndProgress(coldStatusIcon, progress),
                        null);
                }
                else
                {
                    coldStatus.UpdateAppearance(StatusAppearance.IconAndProgress(coldStatusIcon, progress));
                }
            }
            else
            {
                coldStatus?.DeleteImmediately();
                coldStatus = null;
            }

            if (newTemperature > Constants.Game.Elements.MaxNormalTemperature)
            {
                progress = (newTemperature - Constants.Game.Elements.MaxNormalTemperature) /
                    (Constants.Game.Elements.MaxTemperature - Constants.Game.Elements.MaxNormalTemperature);

                if (hotStatus is null)
                {
                    hotStatus = statusTracker.AddStatus(
                        new StatusSpec(StatusType.Neutral, null, hotDrawer),
                        StatusAppearance.IconAndProgress(hotStatusIcon, progress),
                        null);
                }
                else
                {
                    hotStatus.UpdateAppearance(StatusAppearance.IconAndProgress(coldStatusIcon, progress));
                }
            }
            else
            {
                hotStatus?.DeleteImmediately();
                hotStatus = null;
            }
        }

        public void BeginOverheat()
        {
            overheatedStatus = statusTracker.AddStatus(
                new StatusSpec(StatusType.Negative, null, overheatedDrawer),
                StatusAppearance.IconOnly(overheatedStatusIcon),
                null);
        }

        public void EndOverheat()
        {
            overheatedStatus?.DeleteImmediately();
            overheatedStatus = null;
        }
    }
}
