namespace Bearded.TD.Shared.TechEffects;

public struct Modification
{
    public static Modification AddConstant(double constant) => new(ModificationType.AdditiveAbsolute, constant);
    public static Modification AddFractionOfBase(double fraction) => new(ModificationType.AdditiveRelative, fraction);
    public static Modification MultiplyWith(double exponent) => new(ModificationType.Exponent, exponent);

    public enum ModificationType : byte
    {
        // Additive
        AdditiveAbsolute,
        AdditiveRelative,

        // Exponential
        Exponent,
    }

    public ModificationType Type { get; }
    public double Value { get; }

    private Modification(ModificationType type, double value)
    {
        Type = type;
        Value = value;
    }
}
