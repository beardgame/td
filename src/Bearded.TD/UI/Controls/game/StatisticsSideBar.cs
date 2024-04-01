using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls;

sealed class StatisticsSideBar : CompositeControl
{
    public StatisticsSideBar()
    {
        IsClickThrough = true;
        Add(new WaveReportScreen());
    }
}
