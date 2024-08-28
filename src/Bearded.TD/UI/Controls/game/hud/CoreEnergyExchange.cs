using Bearded.TD.Utilities;
using Bearded.UI;

namespace Bearded.TD.UI.Controls;

sealed class CoreEnergyExchange
{
    public (Interval Range, double StepSize) ValidExchangeRates { get; } = (Interval.FromStartAndEnd(0.25, 1.5), 0.05);
    public Binding<double> ExchangeRate { get; } = new(0.75);
}
