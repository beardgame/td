using Bearded.TD.Rendering;
using Bearded.Utilities.Collections;

namespace Bearded.TD.Game.Simulation.Drawing;

interface IRenderable : IDeletable
{
    void Render(CoreDrawers drawers);
}
