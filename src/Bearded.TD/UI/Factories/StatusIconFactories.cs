using Bearded.Graphics;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.UI.Controls;
using Bearded.TD.UI.Shapes;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;

namespace Bearded.TD.UI.Factories;

static class StatusIconFactories
{
    /**
     * Specifications:
     * - This gets called once to create a UI element displaying a status.
     * - Current relevant properties and what they mean:
     *   - Spec: the immutable information about the status that will never change.
     *     - Type: whether the status is neutral, positive, or negative. Should be represented somehow, e.g. by the
     *         colour of the outline.
     *     - Interaction: input-specific information, populated by the component who owns the status.
     *   - Appearance: draw-specific information, populated by the component who owns the status
     *     - Icon: ModAwareSpriteId of the icon to draw
     *     - Progress: a nullable double that indicates the current progress, if any (null means no progress is drawn)
     *   - Expiry: the game timestamp when the current status will automatically be removed
     * - Currently missing properties that will likely be added in the future:
     *   - Tooltip: some kind of specification for a tooltip that's shown on hover
     *   - (Maybe) Progress colour: we may separate progress bar colours, e.g. to clearly distinguish hot and cold
     */
    public static Control StatusIcon(
        this UIFactories factories,
        ObservableStatus status,
        GameRequestDispatcher requestDispatcher,
        Player player)
    {
        var progressOrZero = status.Appearance.Transform(a => a.Progress ?? 0);

        // TODO: replace entirely
        return factories.StandaloneIconButton(b => b
            .WithOnClick(() => status.Spec.Interaction?.Interact(requestDispatcher, player))
            .WithEnabled(Binding.Constant(status.Spec.IsInteractive).And(status.Appearance.Transform(a => a.Enabled)))
            .WithIcon(status.Appearance.Transform(a => a.Icon))
            .WithIconScale(0.75f)
            .WithProgressBar(progressOrZero, 6, progressBarComponent)
            .MakeHexagon());

        static ShapeComponent progressBarComponent(double p, GradientStop[] stops)
        {
            if (p == 0)
            {
                return default;
            }

            var stop1 = 0.5f - p / 2;
            var stop2 = 0.5f + p / 2;
            stops[0] = (0, Color.Transparent);
            stops[0] = (stop1, Color.Transparent);
            stops[1] = (stop1, Color.White);
            stops[2] = (stop2, Color.White);
            stops[3] = (stop2, Color.Transparent);
            stops[4] = (1, Color.Transparent);

            return Edge.Inner(2, ShapeColor.FromMutable(
                stops,
                GradientDefinition.ArcAroundPoint(AnchorPoint.FrameCenter, Direction2.FromDegrees(90), 360.Degrees())
            ));
        }
    }

    /**
     * Specifications:
     * - This gets called once to create a UI element displaying an upgrade slot.
     * - Upgrade slots are immutable records. The binding gets an update notification when the slot is filled by an
     *   upgrade (to allow for animating).
     * - Current relevant properties and what they mean:
     *   - Upgrade: if not null, the upgrade that's been put in this slot.
     * - Future expansions:
     *   - A (potentially partially mutable) property on `UpgradeSlot` denoting the progress of unlocking it.
     *   - A reference to the veterancy rank to which the slot is tied.
     * - The slot is interactive if both Upgrade and UnlockProgress are null. In the future we may distinguish the first
     *   empty upgrade slot from subsequent slots since you should always fill slots from left to right.
     */
    public static Control UpgradeSlot(
        this UIFactories factories,
        IReadonlyBinding<UpgradeSlot> upgradeSlot,
        IReadonlyBinding<bool> isActiveSlot,
        VoidEventHandler onClick)
    {
        var upgrade = upgradeSlot.Transform(slot => slot.Upgrade);
        // TODO: replace entirely
        return factories.Button(b => b
            .forUpgrade(upgrade)
            .WithOnClick(onClick)
            .WithEnabled(isActiveSlot)
            .MakeHexagon());
    }

    public static Control UpgradeChoice(
        this UIFactories factories,
        IPermanentUpgrade upgrade,
        ButtonClickEventHandler onClick,
        IReadonlyBinding<bool> enabled)
    {
        var upgradeBinding = Binding.Constant(upgrade);
        // TODO: replace entirely
        return factories.Button(b => b
            .forUpgrade(upgradeBinding)
            .WithOnClick(onClick)
            .WithEnabled(enabled)
            .MakeHexagon());
    }

    private static ButtonFactory.TextButtonBuilder forUpgrade(
        this ButtonFactory.TextButtonBuilder builder,
        IReadonlyBinding<IPermanentUpgrade?> upgrade)
    {
        return builder
            .WithLabel(upgrade.Transform(u => u?.Name[..1] ?? ""))
            .WithTooltip(upgrade.Transform(u => u?.Name ?? ""));
    }
}
