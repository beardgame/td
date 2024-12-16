using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

interface IStatusInteractionSpec
{
    Keys? ShortcutKey => null;
    void Interact(GameRequestDispatcher requestDispatcher, Player player);
}
