using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.Geometry;

namespace Bearded.TD.Game.Meta;

interface ISelectable
{
    bool IsSelectable { get; }
    GameObject Object { get; }
    Rectangle BoundingBox { get; }

    void ResetSelection();
    void Focus(SelectionManager.UndoDelegate undoFocus);
    void Select(SelectionManager.UndoDelegate undoSelection);
}
