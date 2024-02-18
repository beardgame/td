using System;
using Bearded.Graphics.Shapes;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

sealed class ProgressStatusDrawer(IStatusDrawer inner, Func<float> getProgress) : IStatusDrawer
{
    public void Draw(CoreDrawers core, IComponentDrawer drawer, Vector3 position, float size)
    {
        drawProgress(core, position, size);
        inner.Draw(core, drawer, position, size);
    }

    private void drawProgress(CoreDrawers core, Vector3 position, float size)
    {
        var progress = getProgress().Clamped(0, 1);
        var bottomLeft = position - size * 0.5f * new Vector3(1, 1, 0);
        var wh = new Vector2(size, size * progress);
        core.Primitives.FillRectangle(bottomLeft, wh, Constants.Game.GameUI.StatusDisplay.BackgroundColor);
    }
}
