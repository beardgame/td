namespace Bearded.TD.Game.Simulation.StatusDisplays;

sealed record StatusSpec(
    StatusType Type, IStatusDrawSpec DrawSpec, IStatusInteractionSpec? InteractionSpec, IStatusDrawer Drawer);
