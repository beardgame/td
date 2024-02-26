using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using OpenTK.Mathematics;

namespace Bearded.TD.UI.Controls;

sealed class BuildingStatusControl : CompositeControl
{
    public static readonly Vector2d Size = (100, 20);

    public BuildingStatusControl(BuildingStatus model)
    {
        Add(new BackgroundBox());
        Add(TextFactories.Label(model.ShowExpanded.Transform(b => b ? "Expanded" : "Preview")));
    }
}
