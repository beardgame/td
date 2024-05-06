namespace Bearded.TD.Game.Simulation.StatusDisplays;

sealed record StatusSpec(StatusType Type, IStatusInteractionSpec? Interaction)
{
    public bool IsInteractive => Interaction is not null;
}
