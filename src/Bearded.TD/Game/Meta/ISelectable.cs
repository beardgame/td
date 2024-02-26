using System;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;

namespace Bearded.TD.Game.Meta;

interface ISelectable
{
    bool IsSelectable { get; }
    [Obsolete] IReportSubject Subject { get; }
    GameObject Object { get; }

    void ResetSelection();
    void Focus(SelectionManager.UndoDelegate undoFocus);
    void Select(SelectionManager.UndoDelegate undoSelection);
}
