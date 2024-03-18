using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Factories;

static class StatusIconFactories
{
    /**
     * Specifications:
     * - This gets called once to create a UI element displaying a status
     * - Changes the status can come in two ways:
     *   - Statuses aren't immutable: elements such as the icon and the progress may change frame to frame
     *   - The status may be swapped out entirely: if a status gets removed from the list of statuses, instead of moving
     *     the controls, the content of the statuses is moved over instead.
     *     - COMMENT: This is likely to make animation and effect continuity challenging
     * - Status value may be set to null; however it is not expected at this time the control will continue to be drawn
     *   and you can use it as a signal that you can unsubscribe from the event.
     * - Current relevant properties and what they mean:
     *   - Type: whether the status is neutral, positive, or negative. Should be represented somehow, e.g. by the colour
     *     of the outline.
     *   - DrawSpec: draw-specific information, populated by the component who owns the status
     *     - Icon: ModAwareSpriteId of the icon to draw
     *     - Progress: a nullable double that indicates the current progress, if any (null means no progress is drawn)
     *   - Expiry: not used right now and does not have to be rendered
     * - Currently missing properties that will likely be added in the future:
     *   - Interactive: whether an icon can be clicked to trigger an action
     *   - Tooltip: some kind of specification for a tooltip that's shown on hover
     *   - (Maybe) Progress colour: we may separate progress bar colours, e.g. to clearly distinguish hot and cold
     * - Expected appearance:
     *   - A round (hex in the future?) outlined circle that will be shown on a black semi-transparent overlay on top of
     *     the game
     *   - Progress is shown as a circular progressbar along the inside of the border
     *   - Expected size by the caller is Constants.UI.Button.SquareButtonSize (same as action bar icons)
     *     - COMMENT: They looked much too big to me, I made them half the size
     * - Future expansions:
     *   - We expect the same UI to contain very similar looking icons, but not for statuses
     *     - Upgrades: both upgrades that are currently active, as well as buttons to select a new upgrade
     *     - Empty upgrade slots: both the next slot that can be filled (enabled), further slots that can be filled (can
     *       perhaps not be clicked), and the next slot to be unlocked (disabled, shows next veterancy rank icon with
     *       low transparency and progress to next rank).
     *   - May want to reuse elements of the controls for those as well, since they'll be visually similar
     */
    public static Control StatusIcon(IReadonlyBinding<Status?> status)
    {
        // TODO: replace entirely
        return ButtonFactories.StandaloneIconButton(b => b
            .WithIcon(status.Transform(s => s?.DrawSpec.Icon ?? Constants.Content.CoreUI.Sprites.Technology))
            .WithIconScale(0.75f)
            .MakeHexagon());
    }

    /**
     * Specifications:
     * - See above
     * - Upgrade slots are immutable records, with the exception of the progress which may change per frame
     * - Current relevant properties and what they mean:
     *   - Upgrade: if not null, the upgrade that's been put in this slot.
     *   - UnlockProgress: if not null, the slot is not available yet. The object itself may be mutable and indicate the
     *     current progression towards unlocking this slot.
     * - Future expansions:
     *   - A reference to the veterancy rank to which the slot is tied.
     * - The slot is interactive if both Upgrade and UnlockProgress are null. In the future we may distinguish the first
     *   empty upgrade slot from subsequent slots since you should always fill slots from left to right.
     * - Feel free to add further property getters to the types if that leads to nicer code.
     */
    public static Control UpgradeSlot(IReadonlyBinding<UpgradeSlot?> upgradeSlot)
    {
        // TODO: replace entirely
        return ButtonFactories.Button(b => b
            .WithLabel(upgradeSlot.Transform(slot => slot?.Upgrade?.Name[..1] ?? ""))
            .MakeHexagon());
    }

    /*
     * Open comments:
     * - If we need to make the bindings so that the objects inside are fully immutable and we update the contents even
     *   for currently mutable things such as icon and progress, this would require more work but can be done.
     */
}
