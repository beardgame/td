﻿using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Utilities.Input;
using Bearded.UI.EventArgs;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Input;

interface ICursorHandler
{
    ActionState Click { get; }
    ActionState Cancel { get; }
    Position2 CursorPosition { get; }
    ModifierKeys ModifierKeys { get; }
    PositionedFootprint CurrentFootprint { get; }
    void HandleInput(InputState inputContext);
    void SetTileSelection(TileSelection tileSelection);
    void SetCameraController(ICameraController controller);
    void ResetCameraController();
}
