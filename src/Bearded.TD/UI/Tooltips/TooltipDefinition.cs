using System;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Tooltips;

public readonly record struct TooltipDefinition(Func<Control> CreateControl, double Width, double Height);
