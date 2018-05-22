using System;

namespace Bearded.TD.Utilities.Input
{
    public interface IAction : IEquatable<IAction>
    {
        bool Hit { get; }
        bool Active { get; }
        bool Released { get; }

        bool IsAnalog { get; }
        float AnalogAmount { get; }
    }
}
