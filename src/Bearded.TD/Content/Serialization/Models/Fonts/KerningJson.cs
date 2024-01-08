namespace Bearded.TD.Content.Serialization.Models.Fonts;

readonly record struct KerningJson(
    int Unicode1,
    int Unicode2,
    float Advance);
