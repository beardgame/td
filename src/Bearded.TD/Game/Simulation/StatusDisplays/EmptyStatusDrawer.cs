using System;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Rendering;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

[Obsolete]
sealed class EmptyStatusDrawer : IStatusDrawer
{
    public void Draw(CoreDrawers core, IComponentDrawer drawer, Vector3 position, float size) {}
}
