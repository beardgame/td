namespace Bearded.TD.Game.Simulation.StatusDisplays;

sealed record StatusSpec(StatusType Type, IStatusInteractionSpec? Interaction, IStatusDrawer Drawer)
{
    public bool IsInteractive => Interaction is not null;
}
