using Bearded.TD.Game.Overlays;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Factories;

static class ReportFactories
{
    public static Control TowerDamageDisplay(
        this UIFactories factories,
        TowerDamageDisplay model,
        double? expectedControlHeight = null)
    {
        return factories.ReportFactory.TowerDamageDisplay(model, expectedControlHeight);
    }
}
