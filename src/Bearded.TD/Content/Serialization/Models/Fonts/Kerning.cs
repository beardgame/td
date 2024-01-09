namespace Bearded.TD.Content.Serialization.Models.Fonts;

readonly record struct Kerning(
    int Unicode1,
    int Unicode2,
    float Advance);
