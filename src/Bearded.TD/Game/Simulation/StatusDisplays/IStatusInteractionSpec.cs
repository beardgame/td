using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

interface IStatusInteractionSpec
{
    void Interact(GameRequestDispatcher requestDispatcher, Player player);
}
