using System;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.Utilities.Geometry;

namespace Bearded.TD.Game.Meta;

interface ISelectable
{
    bool IsSelectable { get; }
    [Obsolete] IReportSubject Subject { get; }
    GameObject Object { get; }
    Rectangle BoundingBox { get; }

    void ResetSelection();
    void Focus(SelectionManager.UndoDelegate undoFocus);
    void Select(SelectionManager.UndoDelegate undoSelection);
}
