using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.UI.Animation;
using Bearded.TD.UI.Controls;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Factories;

static class StatusIconFactories
{
    /**
     * Specifications:
     * - This gets called once to create a UI element displaying a status.
     * - Statuses aren't immutable: elements such as the icon and the progress may change frame to frame (since we do
     *   not have good ways to detect changes in the UI).
     * - At this time it is unlikely the status binding will be updated.
     * - Current relevant properties and what they mean:
     *   - Type: whether the status is neutral, positive, or negative. Should be represented somehow, e.g. by the colour
     *     of the outline.
     *   - DrawSpec: draw-specific information, populated by the component who owns the status
     *     - Icon: ModAwareSpriteId of the icon to draw
     *     - Progress: a nullable double that indicates the current progress, if any (null means no progress is drawn)
     *   - InteractionSpec: input-specific information, populated by the component who owns the status
     *   - Expiry: the game timestamp when the current status will automatically be removed
     * - Currently missing properties that will likely be added in the future:
     *   - Interactive: whether an icon can be clicked to trigger an action
     *   - Tooltip: some kind of specification for a tooltip that's shown on hover
     *   - (Maybe) Progress colour: we may separate progress bar colours, e.g. to clearly distinguish hot and cold
     */
    public static Control StatusIcon(IReadonlyBinding<Status> status, Animations animations)
    {
        // TODO: replace entirely
        return ButtonFactories.StandaloneIconButton(b => b
            .WithAnimations(animations)
            .WithOnClick(() => status.Value.InteractionSpec?.Interact())
            .WithInteractive(status.Transform(s => s.CanInteract))
            .WithIcon(status.Transform(s => s.DrawSpec.Icon))
            .WithIconScale(0.75f)
            .MakeHexagon());
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
    public static Control UpgradeSlot(IReadonlyBinding<UpgradeSlot> upgradeSlot, Animations animations)
    {
        // TODO: replace entirely
        return ButtonFactories.Button(b => b
            .WithAnimations(animations)
            .WithInteractive(upgradeSlot.Transform(slot => slot.Upgrade is null))
            .WithLabel(upgradeSlot.Transform(slot => slot.Upgrade?.Name[..1] ?? ""))
            .MakeHexagon());
    }
}
