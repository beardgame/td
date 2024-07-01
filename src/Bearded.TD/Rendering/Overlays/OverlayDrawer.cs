using Bearded.Graphics;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.Shapes;
using Bearded.TD.Game.Overlays;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Constants.Game.World;
using ColorVertexData = Bearded.TD.Rendering.Vertices.ColorVertexData;

namespace Bearded.TD.Rendering.Overlays;

sealed class OverlayDrawer(ExpandingIndexedTrianglesMeshBuilder<ColorVertexData> meshBuilder)
    : IOverlayDrawer
{
    private readonly ShapeDrawer2<ColorVertexData, Color> drawer = new(meshBuilder, ColorVertexData.Create);

    public void Tile(Color color, Tile tile, Unit height = default)
    {
        var p = Level.GetPosition(tile).NumericValue;

        drawer.FillCircle(p.WithZ(height.NumericValue), HexagonSide, color, 6);
    }
}
