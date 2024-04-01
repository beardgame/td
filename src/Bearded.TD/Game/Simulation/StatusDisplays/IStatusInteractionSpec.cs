using Bearded.TD.Game.Commands;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

interface IStatusInteractionSpec
{
    void Interact(GameRequestDispatcher requestDispatcher);
}
