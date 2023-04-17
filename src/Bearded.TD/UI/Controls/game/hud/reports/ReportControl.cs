using System;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls;

abstract class ReportControl : CompositeControl, IDisposable
{
    public abstract double Height { get; }

    public abstract void Update();
    public abstract void Dispose();
}
