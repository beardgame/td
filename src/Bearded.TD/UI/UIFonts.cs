using System.Collections.Frozen;
using System.Collections.Generic;
using Bearded.Graphics;
using Bearded.Graphics.Text;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.Text;
using Bearded.TD.Rendering.Vertices;
using OpenTK.Mathematics;
using static Bearded.TD.Constants.Content.CoreUI;
using static Bearded.TD.Constants.UI;

namespace Bearded.TD.UI;

sealed class UIFonts
{
    private static readonly IReadOnlyDictionary<TextStyle, TextStyleDefinition> textStyles =
        new Dictionary<TextStyle, TextStyleDefinition>
        {
            { TextStyle.Default, new TextStyleDefinition(Fonts.DefaultText, Text.FontSize) },
            { TextStyle.Monospace, new TextStyleDefinition(Fonts.MonospaceText, Text.FontSize) },
        }.ToFrozenDictionary();

    public static UIFonts Load(Blueprints blueprints, IDrawableRenderers renderers)
    {
        var fontDrawers = textStyles.ToFrozenDictionary(
            kvp => kvp.Key,
            kvp =>
            {
                var def = kvp.Value;
                var font = blueprints.Fonts[def.Font];
                var drawer = font
                    .MakeConcreteWith(renderers, DrawOrderGroup.UIFont, 0, UVColorVertex.Create)
                    .WithDefaults(def.DefaultSize, 0, 0, Vector3.UnitX, Vector3.UnitY, Color.White);
                return drawer;
            });
        return new UIFonts(fontDrawers);
    }

    private readonly IReadOnlyDictionary<TextStyle, TextDrawerWithDefaults<Color>> fontDrawers;

    private UIFonts(IReadOnlyDictionary<TextStyle, TextDrawerWithDefaults<Color>> fontDrawers)
    {
        this.fontDrawers = fontDrawers;
    }

    public TextDrawerWithDefaults<Color> Default => ForStyle(TextStyle.Default);

    public TextDrawerWithDefaults<Color> ForStyle(TextStyle style) => fontDrawers[style];

    private sealed record TextStyleDefinition(ModAwareId Font, float DefaultSize);
}
