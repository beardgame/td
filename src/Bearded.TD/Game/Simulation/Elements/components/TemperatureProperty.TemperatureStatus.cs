using Bearded.TD.Game.Simulation.StatusDisplays;

namespace Bearded.TD.Game.Simulation.Elements;

sealed partial class TemperatureProperty
{
    private sealed class TemperatureStatus
    {
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
                coldStatus ??= statusTracker.AddStatus(new StatusSpec(StatusType.Neutral, coldDrawer), null);
                progress = (newTemperature - Constants.Game.Elements.MinNormalTemperature) /
                    (Constants.Game.Elements.MinTemperature - Constants.Game.Elements.MinNormalTemperature);
            }
            else
            {
                coldStatus?.DeleteImmediately();
                coldStatus = null;
            }

            if (newTemperature > Constants.Game.Elements.MaxNormalTemperature)
            {
                hotStatus ??= statusTracker.AddStatus(new StatusSpec(StatusType.Neutral, hotDrawer), null);
                progress = (newTemperature - Constants.Game.Elements.MaxNormalTemperature) /
                    (Constants.Game.Elements.MaxTemperature - Constants.Game.Elements.MaxNormalTemperature);
            }
            else
            {
                hotStatus?.DeleteImmediately();
                hotStatus = null;
            }
        }

        public void BeginOverheat()
        {
            overheatedStatus = statusTracker.AddStatus(new StatusSpec(StatusType.Negative, overheatedDrawer), null);
        }

        public void EndOverheat()
        {
            overheatedStatus?.DeleteImmediately();
            overheatedStatus = null;
        }
    }
}
